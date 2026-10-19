using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Authentication.Jwt;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Core.Util;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Models.Dto;
using System.Security.Claims;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 聊天认证服务 — Widget 端访客认证入口协调者（业务直接依赖 Realtime 框架模块）。
    /// 聚焦访客认证流程编排：平台校验 → 用户建联 → JWT 签发 → 客服分配 → 会话编排 → 登录留痕。
    /// 会话编排委托 ChatSessionOrchestrator，访问校验委托 ChatAccessValidator，SDK 响应映射委托 SdkResponseBuilder。
    /// </summary>
    [AutoDependencyInjection]
    public class ChatAuthService
    {
        private readonly ILogger<ChatAuthService> _logger;
        private readonly ChatPlatformService _platformService;
        private readonly ChatUserService _chatUserService;
        private readonly AgentAllocationService _allocationService;
        private readonly MemberService _memberService;
        private readonly JwtToken _jwtToken;
        private readonly AccessViolationService _violationService;
        private readonly QueueService _queueService;
        private readonly QuotaService _quotaService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ChatSessionOrchestrator _sessionOrchestrator;
        private readonly ChatAccessValidator _accessValidator;

        public ChatAuthService(
            ILogger<ChatAuthService> logger,
            ChatPlatformService platformService,
            ChatUserService chatUserService,
            AgentAllocationService allocationService,
            MemberService memberService,
            JwtToken jwtToken,
            AccessViolationService violationService,
            QueueService queueService,
            QuotaService quotaService,
            IHttpContextAccessor httpContextAccessor,
            ChatSessionOrchestrator sessionOrchestrator,
            ChatAccessValidator accessValidator)
        {
            _logger = logger;
            _platformService = platformService;
            _chatUserService = chatUserService;
            _allocationService = allocationService;
            _memberService = memberService;
            _jwtToken = jwtToken;
            _violationService = violationService;
            _queueService = queueService;
            _quotaService = quotaService;
            _httpContextAccessor = httpContextAccessor;
            _sessionOrchestrator = sessionOrchestrator;
            _accessValidator = accessValidator;
        }

        /// <summary>
        /// 根据 PlatformKey 获取平台（公开方法，供 Controller 调用）
        /// </summary>
        public ChatPlatform GetPlatformByKey(string platformKey)
        {
            return _platformService.GetByPlatformKey(platformKey);
        }

        public async Task<ResultData> VisitorAuthAsync(VisitorAuthRequest request)
        {
            // 1. 验证平台
            var platform = _platformService.GetByPlatformKey(request.PlatformKey);
            if (platform == null)
                return ResultData.No("无效的平台秘钥!");

            if (!platform.IsEnabled)
            {
                _violationService.Log(platform.Id, request.PlatformKey, platform.OwnerId,
                    request.RequestDomain, request.Source ?? "unknown", platform.AccessType.ToString().ToLower(),
                    "platform_disabled", request.IpAddress, request.UserAgent);
                return ResultData.No("该平台已停用!");
            }

            // 1.5 接入来源校验
            var validationResult = _accessValidator.ValidateAccess(platform, request);
            if (validationResult != null)
                return validationResult;

            // 1.6 每日访客数限制
            var dailyCheck = _quotaService.CheckQuota(platform.OwnerId, "max_daily_visitors");
            if (!dailyCheck.IsAvailable)
            {
                _logger.LogWarning("每日访客数超限: OwnerId={OwnerId}, Used={Used}, Limit={Limit}",
                    platform.OwnerId, dailyCheck.Used, dailyCheck.Limit);
                return ResultData.No("今日咨询量已达上限，请明日再来。");
            }

            // 2. 查找或创建聊天用户
            ChatUser? chatUser = null;

            // 优先按 ClientId 查找
            if (!string.IsNullOrEmpty(request.ClientId))
            {
                chatUser = _chatUserService.GetByClientId(request.ClientId, platform.Id);
            }

            // 按 ExternalUserId 查找
            if (chatUser == null && !string.IsNullOrEmpty(request.ExternalUserId))
            {
                chatUser = _chatUserService.GetByExternalUserId(request.ExternalUserId, platform.Id);
            }

            bool isNewUser = false;
            if (chatUser == null)
            {
                isNewUser = true;
                chatUser = new ChatUser
                {
                    ChatUserId = RealtimeIdGenerator.GenerateChatUserId(),
                    UserType = string.IsNullOrEmpty(request.ExternalUserId) ? ChatUserType.Anonymous : ChatUserType.Authenticated,
                    PlatformId = platform.Id,
                    OwnerId = platform.OwnerId,
                    ExternalUserId = request.ExternalUserId,
                    Username = request.Username,
                    Nickname = request.Nickname ?? $"访客_{DateTime.Now:HHmmss}",
                    Avatar = request.Avatar,
                    ClientId = request.ClientId,
                    DeviceType = request.DeviceType,
                    UserAgent = request.UserAgent,
                    Status = ChatUserStatus.Normal,
                    OnlineStatus = OnlineStatus.Online,
                    FirstVisitTime = DateTime.Now,
                    LastActiveTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    IsDeleted = false
                };

                chatUser = _chatUserService.CreateChatUser(chatUser);
            }
            else
            {
                // 更新在线状态和最后活跃时间
                chatUser.OnlineStatus = OnlineStatus.Online;
                chatUser.LastActiveTime = DateTime.Now;
                if (!string.IsNullOrEmpty(request.DeviceType)) chatUser.DeviceType = request.DeviceType;
                if (!string.IsNullOrEmpty(request.UserAgent)) chatUser.UserAgent = request.UserAgent;

                _chatUserService.UpdateColumns(chatUser, c => new { c.OnlineStatus, c.LastActiveTime, c.DeviceType, c.UserAgent });
            }

            // 3. 为访客签发 JWT（Realtime Hub 连接鉴权用；复用 JwtAdmin 验签，claims 标识访客）
            var jwt = IssueVisitorJwt(chatUser, platform);

            // 4. 分配客服（已有分配则直接复用，避免排队后接入又重新分配）
            AgentAllocationResult allocation;

            if (chatUser.AssignedAgentId.HasValue && chatUser.AssignedAgentId.Value > 0)
            {
                // 已有分配的客服（如从排队中接入），直接复用
                var existingAgent = _memberService.GetActiveById(chatUser.AssignedAgentId.Value);

                if (existingAgent != null)
                {
                    allocation = new AgentAllocationResult
                    {
                        Success = true,
                        AgentOnline = existingAgent.OnlineStatus != OnlineStatus.Offline,
                        AgentId = existingAgent.Id,
                        AgentName = existingAgent.Name ?? existingAgent.Username,
                        AgentAvatar = existingAgent.Avatar.ToAbsoluteUrl(_httpContextAccessor.HttpContext!),
                        RealtimeUserId = existingAgent.RealtimeUserId,
                        WelcomeMessage = existingAgent.WelcomeMessage
                    };
                    _logger.LogInformation("复用已分配客服: ChatUserId={ChatUserId}, AgentId={AgentId}",
                        chatUser.ChatUserId, existingAgent.Id);
                }
                else
                {
                    // 已分配的客服不存在或已禁用，重新分配
                    allocation = _allocationService.AllocateAgent(platform.OwnerId, platform.Id);
                }
            }
            else
            {
                allocation = _allocationService.AllocateAgent(platform.OwnerId, platform.Id);
            }

            // 4.1 如果需要排队（并发会话超限）
            if (!allocation.Success && allocation.NeedsQueue)
            {
                var queueEntry = _queueService.Enqueue(chatUser.ChatUserId, platform.Id, platform.OwnerId);
                var positionInfo = _queueService.GetPosition(chatUser.ChatUserId);

                // 返回 JWT 让客户端能连接 Realtime Hub，但标记排队状态
                return ResultData.Ok(new VisitorAuthResponse
                {
                    ChatUserId = chatUser.ChatUserId,
                    Token = jwt.Token,
                    TokenExpire = jwt.ExpireSeconds,
                    Nickname = chatUser.Nickname,
                    Avatar = chatUser.Avatar,
                    IsQueued = true,
                    QueuePosition = positionInfo?.Position ?? 0,
                    QueueWaitingCount = positionInfo?.TotalWaiting ?? 0,
                    EstimatedWaitMinutes = positionInfo?.EstimatedWaitMinutes ?? 0,
                    PlatformName = platform.PlatformName,
                    OfflineMessage = allocation.Message
                });
            }

            // 确保客服有 Realtime 用户 ID（Realtime userId）
            _sessionOrchestrator.EnsureAgentUserId(allocation);

            if (allocation.Success && chatUser.AssignedAgentId != allocation.AgentId)
            {
                chatUser.AssignedAgentId = allocation.AgentId;
                _chatUserService.UpdateColumns(chatUser, c => c.AssignedAgentId!);
            }

            // 5. 创建会话（统一群聊模式 → Realtime group 会话）
            string conversationId = chatUser.GroupId;
            if (allocation.Success && !string.IsNullOrEmpty(allocation.RealtimeUserId) && string.IsNullOrEmpty(conversationId))
            {
                conversationId = await _sessionOrchestrator.CreateSessionConversationAsync(chatUser, allocation, platform);
            }

            // 6. 记录登录
            _accessValidator.RecordLogin(chatUser, request);

            // 7. 发送 session_init（必须 await，客户端依赖会话已存在）
            if (allocation.Success && !string.IsNullOrEmpty(allocation.RealtimeUserId) && !string.IsNullOrEmpty(conversationId))
            {
                var reason = isNewUser ? "login" : "reconnect";
                await _sessionOrchestrator.SendGroupSessionInitAsync(allocation.RealtimeUserId, conversationId, reason);
            }

            // 8. 新用户发送欢迎语（fire-and-forget）
            if (isNewUser && allocation.Success && !string.IsNullOrEmpty(allocation.WelcomeMessage) && !string.IsNullOrEmpty(conversationId))
            {
                _ = _sessionOrchestrator.SendGroupTextAsync(allocation.RealtimeUserId, conversationId, allocation.WelcomeMessage);
            }

            return ResultData.Ok(new VisitorAuthResponse
            {
                ChatUserId = chatUser.ChatUserId,
                Token = jwt.Token,
                TokenExpire = jwt.ExpireSeconds,
                Nickname = chatUser.Nickname,
                Avatar = chatUser.Avatar,
                AgentId = allocation.AgentId,
                AgentName = allocation.AgentName,
                AgentAvatar = allocation.AgentAvatar.ToAbsoluteUrl(_httpContextAccessor.HttpContext!),
                AgentUserId = allocation.RealtimeUserId,
                AgentOnline = allocation.AgentOnline,
                OfflineMessage = !allocation.AgentOnline ? allocation.Message : "",
                WelcomeMessage = allocation.WelcomeMessage,
                PlatformName = platform.PlatformName,
                GroupId = conversationId
            });
        }

        /// <summary>
        /// 刷新访客 Token（重新签发 JWT）
        /// </summary>
        public async Task<ResultData> RefreshTokenAsync(string chatUserId)
        {
            if (string.IsNullOrEmpty(chatUserId))
                return ResultData.No("ChatUserId 不能为空!");

            var chatUser = _chatUserService.GetByChatUserId(chatUserId);
            if (chatUser == null)
                return ResultData.No("用户不存在!");

            var platform = _platformService.GetById(chatUser.PlatformId);
            var jwt = platform != null ? IssueVisitorJwt(chatUser, platform) : IssueVisitorJwt(chatUser, new ChatPlatform { OwnerId = chatUser.OwnerId, Id = chatUser.PlatformId, PlatformKey = "" });

            return ResultData.Ok(new TokenResponse
            {
                Token = jwt.Token,
                ExpireTimeSeconds = jwt.ExpireSeconds
            });
        }

        /// <summary>
        /// 获取客服的连接 JWT（管理端调用；客服用其登录鉴权连 Realtime Hub）
        /// </summary>
        public async Task<ResultData> GetAgentTokenAsync(int memberId, bool forceNew = false)
        {
            var member = _memberService.GetById(memberId);
            if (member == null)
                return ResultData.No("用户不存在!");

            string agentUserId = member.RealtimeUserId;
            if (string.IsNullOrEmpty(agentUserId))
            {
                agentUserId = RealtimeIdGenerator.GenerateAgentId(memberId);
                _memberService.UpdateRealtimeUserId(member.Id, agentUserId);
            }

            var token = _jwtToken.CreateToken("JwtAdmin", new List<Claim>
            {
                new Claim("Id", member.Id.ToString()),
                new Claim("role", "agent"),
                new Claim("agentId", agentUserId),
            });

            return ResultData.Ok(new
            {
                userId = agentUserId,
                token,
                expireTimeSeconds = ReadJwtExpireSeconds("JwtAdmin"),
                realtimeConfig = SdkResponseBuilder.BuildRealtimeConfig(_httpContextAccessor.HttpContext)
            });
        }

        /// <summary>
        /// 刷新会话 — 容灾恢复：重新签发 JWT + 重新分配客服
        /// </summary>
        public async Task<ResultData> RefreshSessionAsync(RefreshSessionRequest request)
        {
            if (string.IsNullOrEmpty(request.ChatUserId))
                return ResultData.No("ChatUserId 不能为空!");

            var chatUser = _chatUserService.GetByChatUserId(request.ChatUserId);
            if (chatUser == null)
                return ResultData.No("用户不存在!");

            // 验证 clientId 匹配
            if (!string.IsNullOrEmpty(request.ClientId) && chatUser.ClientId != request.ClientId)
                return ResultData.No("客户端标识不匹配!");

            // 1. 重新签发访客 JWT
            var platform = _platformService.GetById(chatUser.PlatformId);
            var jwt = platform != null ? IssueVisitorJwt(chatUser, platform) : IssueVisitorJwt(chatUser, new ChatPlatform { OwnerId = chatUser.OwnerId, Id = chatUser.PlatformId, PlatformKey = "" });

            // 2. 重新执行分配逻辑（含滑落）
            string? agentUserId = null;
            bool agentOnline = false;

            if (platform != null)
            {
                var allocation = _allocationService.AllocateAgent(platform.OwnerId, platform.Id);
                _sessionOrchestrator.EnsureAgentUserId(allocation);

                if (allocation.Success)
                {
                    agentUserId = allocation.RealtimeUserId;
                    agentOnline = allocation.AgentOnline;

                    // 如果分配的客服变了，更新
                    if (chatUser.AssignedAgentId != allocation.AgentId)
                    {
                        chatUser.AssignedAgentId = allocation.AgentId;
                        _chatUserService.UpdateColumns(chatUser, c => c.AssignedAgentId!);
                    }
                }
            }

            // 如果重新分配没有结果，保留原来的客服
            if (string.IsNullOrEmpty(agentUserId) && chatUser.AssignedAgentId.HasValue)
            {
                var agent = _memberService.GetById(chatUser.AssignedAgentId.Value);
                if (agent != null)
                {
                    if (string.IsNullOrEmpty(agent.RealtimeUserId))
                    {
                        var newUserId = RealtimeIdGenerator.GenerateAgentId(agent.Id);
                        _memberService.UpdateRealtimeUserId(agent.Id, newUserId);
                        agentUserId = newUserId;
                    }
                    else
                    {
                        agentUserId = agent.RealtimeUserId;
                    }
                    agentOnline = agent.OnlineStatus != OnlineStatus.Offline;
                }
            }

            // 更新在线状态
            chatUser.OnlineStatus = OnlineStatus.Online;
            chatUser.LastActiveTime = DateTime.Now;
            _chatUserService.UpdateColumns(chatUser, c => new { c.OnlineStatus, c.LastActiveTime });

            return ResultData.Ok(new RefreshSessionResponse
            {
                Token = jwt.Token,
                ExpireTimeSeconds = jwt.ExpireSeconds,
                AgentUserId = agentUserId ?? "",
                AgentOnline = agentOnline
            });
        }

        #region Private

        /// <summary>
        /// 为访客签发 JWT（Realtime Hub 连接鉴权；JwtAdmin 验签，claims 标识访客身份与归属）
        /// </summary>
        private JwtPayload IssueVisitorJwt(ChatUser user, ChatPlatform platform)
        {
            var token = _jwtToken.CreateToken("JwtAdmin", new List<Claim>
            {
                new Claim("Id", user.ChatUserId),
                new Claim("role", "visitor"),
                new Claim("uid", user.Id.ToString()),
                new Claim("platformId", platform?.Id.ToString() ?? user.PlatformId.ToString()),
            });

            return new JwtPayload
            {
                Token = token,
                ExpireSeconds = ReadJwtExpireSeconds("JwtAdmin")
            };
        }

        private class JwtPayload
        {
            public string Token { get; set; } = null!;
            public long ExpireSeconds { get; set; }
        }

        /// <summary>读取 JWT 过期配置（分钟→秒）。</summary>
        private long ReadJwtExpireSeconds(string jwtConfigKey)
        {
            var cfg = AppGlobal.AppSettings?.JwtAdmin;
            var expiresStr = jwtConfigKey == "JwtAdmin" ? cfg?.Expires : null;
            return int.TryParse(expiresStr, out var minutes) ? minutes * 60L : 120 * 60L;
        }

        #endregion
    }
}
