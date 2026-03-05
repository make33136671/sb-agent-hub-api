using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Util;
using SbAgentHub.Models;
using SbAgentHub.Models.Dto;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 聊天认证服务 — Widget 端访客认证
    /// </summary>
    [AutoDependencyInjection]
    public class ChatAuthService
    {
        private readonly IFreeSql _fsql;
        private readonly ILogger<ChatAuthService> _logger;
        private readonly ChatPlatformService _platformService;
        private readonly ChatUserService _chatUserService;
        private readonly AgentAllocationService _allocationService;
        private readonly OpenImUserService _openImUserService;
        private readonly OpenImAuthService _openImAuthService;
        private readonly OpenImMessageService _openImMessageService;
        private readonly AccessViolationService _violationService;
        private readonly QueueService _queueService;
        private readonly QuotaService _quotaService;
        private readonly VisitorFormService _visitorFormService;
        private readonly CustomerServiceConfigService _configService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatAuthService(
            IFreeSql fsql,
            ILogger<ChatAuthService> logger,
            ChatPlatformService platformService,
            ChatUserService chatUserService,
            AgentAllocationService allocationService,
            OpenImUserService openImUserService,
            OpenImAuthService openImAuthService,
            OpenImMessageService openImMessageService,
            AccessViolationService violationService,
            QueueService queueService,
            QuotaService quotaService,
            VisitorFormService visitorFormService,
            CustomerServiceConfigService configService,
            IHttpContextAccessor httpContextAccessor)
        {
            _fsql = fsql;
            _logger = logger;
            _platformService = platformService;
            _chatUserService = chatUserService;
            _allocationService = allocationService;
            _openImUserService = openImUserService;
            _openImAuthService = openImAuthService;
            _openImMessageService = openImMessageService;
            _violationService = violationService;
            _queueService = queueService;
            _quotaService = quotaService;
            _visitorFormService = visitorFormService;
            _configService = configService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 访客认证 — Widget 初始化时调用
        /// </summary>
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
            var validationResult = ValidateAccess(platform, request);
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
            ChatUser chatUser = null;

            // 优先按 ClientId 查找
            if (!string.IsNullOrEmpty(request.ClientId))
            {
                chatUser = _chatUserService.GetByClientId(request.ClientId, platform.Id);
            }

            // 按 ExternalUserId 查找
            if (chatUser == null && !string.IsNullOrEmpty(request.ExternalUserId))
            {
                chatUser = _fsql.Select<ChatUser>()
                    .Where(c => c.ExternalUserId == request.ExternalUserId && c.PlatformId == platform.Id)
                    .First();
            }

            bool isNewUser = false;
            if (chatUser == null)
            {
                isNewUser = true;
                chatUser = new ChatUser
                {
                    ChatUserId = OpenImIdGenerator.GenerateChatUserId(),
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

                _fsql.Insert(chatUser).ExecuteAffrows();
                // 重新查询获取自增 ID
                chatUser = _chatUserService.GetByChatUserId(chatUser.ChatUserId);
            }
            else
            {
                // 更新在线状态和最后活跃时间
                chatUser.OnlineStatus = OnlineStatus.Online;
                chatUser.LastActiveTime = DateTime.Now;
                if (!string.IsNullOrEmpty(request.DeviceType)) chatUser.DeviceType = request.DeviceType;
                if (!string.IsNullOrEmpty(request.UserAgent)) chatUser.UserAgent = request.UserAgent;

                _fsql.Update<ChatUser>().SetSource(chatUser)
                    .UpdateColumns(c => new { c.OnlineStatus, c.LastActiveTime, c.DeviceType, c.UserAgent })
                    .ExecuteAffrows();
            }

            // 3. 注册 OpenIM 用户并获取 Token
            var imTokenData = await _openImUserService.RegisterAndGetTokenAsync(
                chatUser.ChatUserId,
                chatUser.Nickname ?? "访客",
                chatUser.Avatar ?? "");

            // 4. 分配客服（已有分配则直接复用，避免排队后接入又重新分配）
            AgentAllocationResult allocation;

            if (chatUser.AssignedAgentId.HasValue && chatUser.AssignedAgentId.Value > 0)
            {
                // 已有分配的客服（如从排队中接入），直接复用
                var existingAgent = _fsql.Select<Member>()
                    .Where(c => c.Id == chatUser.AssignedAgentId.Value && c.Status == UserStatus.Active)
                    .First();

                if (existingAgent != null)
                {
                    allocation = new AgentAllocationResult
                    {
                        Success = true,
                        AgentOnline = existingAgent.OnlineStatus != OnlineStatus.Offline,
                        AgentId = existingAgent.Id,
                        AgentName = existingAgent.Name ?? existingAgent.Username,
                        AgentAvatar = existingAgent.Avatar,
                        OpenImUserId = existingAgent.OpenImUserId,
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

                // 仍然返回 Token 让客户端能连接 WebSocket，但标记排队状态
                return ResultData.Ok(new VisitorAuthResponse
                {
                    ChatUserId = chatUser.ChatUserId,
                    ImToken = imTokenData?.Token,
                    ImTokenExpire = imTokenData?.ExpireTimeSeconds ?? 0,
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

            // 确保客服已注册到 OpenIM
            await EnsureAgentRegisteredAsync(allocation);

            if (allocation.Success && chatUser.AssignedAgentId != allocation.AgentId)
            {
                chatUser.AssignedAgentId = allocation.AgentId;
                _fsql.Update<ChatUser>().SetSource(chatUser)
                    .UpdateColumns(c => c.AssignedAgentId)
                    .ExecuteAffrows();
            }

            // 5. 记录登录
            RecordLogin(chatUser, request);

            // 6. 发送 session_init 确保 OpenIM 会话创建（必须 await，客户端依赖会话已存在）
            if (allocation.Success && !string.IsNullOrEmpty(allocation.OpenImUserId))
            {
                var reason = isNewUser ? "login" : "reconnect";
                await _openImMessageService.SendSessionInitMessageAsync(
                    allocation.OpenImUserId, chatUser.ChatUserId, reason);
            }

            // 7. 新用户发送欢迎语（会话已创建，fire-and-forget 即可）
            if (isNewUser && allocation.Success && !string.IsNullOrEmpty(allocation.WelcomeMessage))
            {
                _ = _openImMessageService.SendWelcomeMessageAsync(
                    allocation.OpenImUserId, chatUser.ChatUserId, allocation.WelcomeMessage);
            }

            return ResultData.Ok(new VisitorAuthResponse
            {
                ChatUserId = chatUser.ChatUserId,
                ImToken = imTokenData?.Token,
                ImTokenExpire = imTokenData?.ExpireTimeSeconds ?? 0,
                Nickname = chatUser.Nickname,
                Avatar = chatUser.Avatar,
                AgentId = allocation.AgentId,
                AgentName = allocation.AgentName,
                AgentAvatar = allocation.AgentAvatar,
                AgentImUserId = allocation.OpenImUserId,
                AgentOnline = allocation.AgentOnline,
                OfflineMessage = !allocation.AgentOnline ? allocation.Message : null,
                WelcomeMessage = allocation.WelcomeMessage,
                PlatformName = platform.PlatformName
            });
        }

        /// <summary>
        /// SDK 端获取 IM Token（适配 openim-core-js SDK 的接口格式）
        /// </summary>
        public async Task<ResultData> GetImTokenForSdkAsync(GetImTokenFormRequest request,
            string requestDomain = "", string ipAddress = "", string userAgent = "")
        {
            // 映射为 VisitorAuthRequest
            var visitorRequest = new VisitorAuthRequest
            {
                PlatformKey = request.PlatformKey,
                ClientId = request.ClientId,
                ExternalUserId = request.UserId,
                Nickname = request.Nickname,
                Avatar = request.Avatar,
                DeviceType = request.PlatformType.ToString(),
                Source = request.Source,
                RequestDomain = requestDomain,
                IpAddress = ipAddress,
                UserAgent = userAgent,
            };

            // 调用核心认证逻辑
            var result = await VisitorAuthAsync(visitorRequest);
            if (result.Code != 0) return result;

            var authResponse = result.Data as VisitorAuthResponse;
            if (authResponse == null) return ResultData.No("认证失败!");

            // 查询 ChatUser 获取更多信息
            var chatUser = _chatUserService.GetByChatUserId(authResponse.ChatUserId);

            // 映射为 SDK 期望的嵌套格式
            var sdkResponse = new GetImTokenSdkResponse
            {
                UserID = authResponse.ChatUserId,
                Token = authResponse.ImToken,
                ExpireTime = authResponse.ImTokenExpire,
                User = new SdkUserProfile
                {
                    UserId = authResponse.ChatUserId,
                    UserType = chatUser != null ? (int)chatUser.UserType : 0,
                    IsNewUser = false,
                    Nickname = authResponse.Nickname ?? "访客",
                    Avatar = authResponse.Avatar ?? "",
                    ExternalUserId = request.UserId,
                    DeviceId = request.ClientId,
                },
                Agent = authResponse.AgentImUserId != null ? new SdkAgentInfo
                {
                    AgentUserId = authResponse.AgentImUserId,
                    Nickname = authResponse.AgentName ?? "客服",
                    Avatar = authResponse.AgentAvatar ?? "",
                    OnlineStatus = authResponse.AgentOnline ? 1 : 0,
                    IsOnline = authResponse.AgentOnline,
                    WelcomeMessage = authResponse.WelcomeMessage,
                    AutoSendWelcome = !string.IsNullOrEmpty(authResponse.WelcomeMessage),
                    ConversationId = $"si_{authResponse.AgentImUserId}_{authResponse.ChatUserId}",
                } : null,
                Platform = new SdkPlatformInfo
                {
                    PlatformId = chatUser?.PlatformId ?? 0,
                    PlatformName = authResponse.PlatformName ?? "",
                    OfflineMessage = authResponse.OfflineMessage,
                    AllowOfflineMessage = true,
                },
                Features = BuildFeatureFlags(chatUser?.OwnerId ?? 0),
                Queue = authResponse.IsQueued ? new SdkQueueInfo
                {
                    IsQueued = true,
                    Position = authResponse.QueuePosition,
                    TotalWaiting = authResponse.QueueWaitingCount,
                    EstimatedWaitMinutes = authResponse.EstimatedWaitMinutes
                } : null,
                VisitorForm = BuildVisitorForm(chatUser?.OwnerId ?? 0, chatUser?.PlatformId ?? 0),
                ImConfig = BuildImConfig()
            };

            return ResultData.Ok(sdkResponse);
        }

        /// <summary>
        /// 刷新 OpenIM Token
        /// </summary>
        public async Task<ResultData> RefreshImTokenAsync(string chatUserId)
        {
            if (string.IsNullOrEmpty(chatUserId))
                return ResultData.No("ChatUserId 不能为空!");

            var chatUser = _chatUserService.GetByChatUserId(chatUserId);
            if (chatUser == null)
                return ResultData.No("用户不存在!");

            var tokenData = await _openImAuthService.GetUserTokenAsync(chatUserId);
            if (tokenData == null)
            {
                _logger.LogWarning("RefreshImToken 获取 Token 失败: ChatUserId={ChatUserId}", chatUserId);
                return ResultData.No("获取 Token 失败!");
            }

            return ResultData.Ok(new ImTokenResponse
            {
                Token = tokenData.Token,
                ExpireTimeSeconds = tokenData.ExpireTimeSeconds
            });
        }

        /// <summary>
        /// 获取客服的 OpenIM Token（管理端调用）
        /// </summary>
        public async Task<ResultData> GetAgentImTokenAsync(int memberId, bool forceNew = false)
        {
            var member = _fsql.Select<Member>().Where(c => c.Id == memberId).First();
            if (member == null)
                return ResultData.No("用户不存在!");

            string imUserId = member.OpenImUserId;
            if (string.IsNullOrEmpty(imUserId))
            {
                imUserId = OpenImIdGenerator.GenerateAgentId(memberId);
                member.OpenImUserId = imUserId;
                _fsql.Update<Member>().SetSource(member).UpdateColumns(c => c.OpenImUserId).ExecuteAffrows();
            }

            // 确保用户已注册到 OpenIM（幂等操作，已存在不会报错）
            await _openImUserService.RegisterUserAsync(imUserId, member.Name ?? member.Username, member.Avatar ?? "");

            // 清除缓存登录：先踢掉旧会话
            if (forceNew)
            {
                await _openImAuthService.ForceLogoutAsync(imUserId);
            }

            var tokenData = await _openImAuthService.GetUserTokenAsync(imUserId);
            if (tokenData == null)
            {
                _logger.LogWarning("GetAgentImToken 获取 Token 失败: MemberId={MemberId}, ImUserId={ImUserId}", memberId, imUserId);
                return ResultData.No("获取 Token 失败!");
            }

            var imConfig = BuildImConfig();
            return ResultData.Ok(new
            {
                imUserId,
                token = tokenData.Token,
                expireTimeSeconds = tokenData.ExpireTimeSeconds,
                imConfig
            });
        }

        /// <summary>
        /// 刷新会话 — 容灾恢复：重新获取 Token + 重新分配客服
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

            // 1. 重新获取 OpenIM Token
            var tokenData = await _openImAuthService.GetUserTokenAsync(chatUser.ChatUserId);
            if (tokenData == null)
            {
                _logger.LogWarning("RefreshSession 获取 Token 失败: ChatUserId={ChatUserId}", chatUser.ChatUserId);
                return ResultData.No("获取 Token 失败!");
            }

            // 2. 查找平台信息用于重新分配
            ChatPlatform platform = null;
            if (!string.IsNullOrEmpty(request.PlatformKey))
            {
                platform = _platformService.GetByPlatformKey(request.PlatformKey);
            }
            if (platform == null)
            {
                platform = _fsql.Select<ChatPlatform>()
                    .Where(c => c.Id == chatUser.PlatformId)
                    .First();
            }

            // 3. 重新执行分配逻辑（含滑落）
            string agentImUserId = null;
            bool agentOnline = false;

            if (platform != null)
            {
                var allocation = _allocationService.AllocateAgent(platform.OwnerId, platform.Id);

                // 确保客服已注册到 OpenIM
                await EnsureAgentRegisteredAsync(allocation);

                if (allocation.Success)
                {
                    agentImUserId = allocation.OpenImUserId;
                    agentOnline = allocation.AgentOnline;

                    // 如果分配的客服变了，更新
                    if (chatUser.AssignedAgentId != allocation.AgentId)
                    {
                        chatUser.AssignedAgentId = allocation.AgentId;
                        _fsql.Update<ChatUser>().SetSource(chatUser)
                            .UpdateColumns(c => c.AssignedAgentId)
                            .ExecuteAffrows();
                    }
                }
            }

            // 如果重新分配没有结果，保留原来的客服
            if (string.IsNullOrEmpty(agentImUserId) && chatUser.AssignedAgentId.HasValue)
            {
                var agent = _fsql.Select<Member>()
                    .Where(c => c.Id == chatUser.AssignedAgentId.Value)
                    .First();
                if (agent != null)
                {
                    // 原客服也需确保已注册 OpenIM
                    if (string.IsNullOrEmpty(agent.OpenImUserId))
                    {
                        var imUserId = OpenImIdGenerator.GenerateAgentId(agent.Id);
                        await _openImUserService.RegisterUserAsync(
                            imUserId, agent.Name ?? agent.Username, agent.Avatar ?? "");
                        agent.OpenImUserId = imUserId;
                        _fsql.Update<Member>().SetSource(agent)
                            .UpdateColumns(c => c.OpenImUserId)
                            .ExecuteAffrows();
                    }
                    agentImUserId = agent.OpenImUserId;
                    agentOnline = agent.OnlineStatus != OnlineStatus.Offline;
                }
            }

            // 更新在线状态
            chatUser.OnlineStatus = OnlineStatus.Online;
            chatUser.LastActiveTime = DateTime.Now;
            _fsql.Update<ChatUser>().SetSource(chatUser)
                .UpdateColumns(c => new { c.OnlineStatus, c.LastActiveTime })
                .ExecuteAffrows();

            return ResultData.Ok(new RefreshSessionResponse
            {
                Token = tokenData.Token,
                ExpireTimeSeconds = tokenData.ExpireTimeSeconds,
                AgentImUserId = agentImUserId,
                AgentOnline = agentOnline
            });
        }

        #region Private

        /// <summary>
        /// 构建 IM 服务器连接配置（从 appsettings 读取，下发给客户端）
        /// </summary>
        private SdkImConfig BuildImConfig()
        {
            var config = AppGlobal.AppSettings?.OpenIM;
            var clientApi = !string.IsNullOrEmpty(config?.ClientApiUrl) ? config.ClientApiUrl : config?.ImApiUrl ?? "";
            var clientWs = !string.IsNullOrEmpty(config?.ClientWsUrl) ? config.ClientWsUrl : config?.ImWsUrl ?? "";

            // 相对地址自动拼接当前请求域名
            if (!string.IsNullOrEmpty(clientApi) && clientApi.StartsWith('/'))
            {
                var baseUrl = GetRequestBaseUrl();
                clientApi = baseUrl + clientApi;
            }
            if (!string.IsNullOrEmpty(clientWs) && clientWs.StartsWith('/'))
            {
                var baseUrl = GetRequestBaseUrl();
                var wsScheme = baseUrl.StartsWith("https") ? "wss" : "ws";
                clientWs = wsScheme + baseUrl.Substring(baseUrl.IndexOf("://")) + clientWs;
            }

            return new SdkImConfig
            {
                ApiUrl = clientApi,
                WsUrl = clientWs
            };
        }

        /// <summary>
        /// 获取当前请求的基础 URL（scheme://host）
        /// </summary>
        private string GetRequestBaseUrl()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "";
            return $"{context.Request.Scheme}://{context.Request.Host}";
        }

        /// <summary>
        /// 构建功能开关
        /// </summary>
        private SdkFeatureFlags BuildFeatureFlags(int ownerId)
        {
            if (ownerId <= 0) return new SdkFeatureFlags();
            var csConfig = _configService.GetEffectiveConfig(ownerId);
            return new SdkFeatureFlags
            {
                EnableSatisfactionRating = _quotaService.CheckFeature(ownerId, "feature_satisfaction"),
                EnableVisitorForm = _quotaService.CheckFeature(ownerId, "feature_visitor_form"),
                EnableQueue = csConfig.EnableQueue,
            };
        }

        /// <summary>
        /// 构建访客表单配置
        /// </summary>
        private SdkVisitorFormConfig BuildVisitorForm(int ownerId, int platformId)
        {
            if (ownerId <= 0) return null;
            if (!_quotaService.CheckFeature(ownerId, "feature_visitor_form")) return null;

            var form = _visitorFormService.GetActiveForm(ownerId, platformId);
            if (form == null) return null;

            return new SdkVisitorFormConfig
            {
                FormId = form.Id,
                FormName = form.FormName,
                FormFields = form.FormFields,
                IsRequired = form.IsRequired
            };
        }

        private ResultData ValidateAccess(ChatPlatform platform, VisitorAuthRequest request)
        {
            string source = request.Source;

            // 兼容旧版本（不带 source 参数）：默认 js，过渡期不强制校验
            if (string.IsNullOrEmpty(source))
                return null;

            // 接入类型校验
            string expectedSource = platform.AccessType.ToString().ToLower();
            if (source.ToLower() != expectedSource)
            {
                _violationService.Log(platform.Id, platform.PlatformKey, platform.OwnerId,
                    request.RequestDomain, source, expectedSource,
                    "type_mismatch", request.IpAddress, request.UserAgent);

                _logger.LogWarning("接入类型不匹配: PlatformKey={PlatformKey}, Expected={Expected}, Got={Got}",
                    platform.PlatformKey, expectedSource, source);

                return ResultData.No("接入方式不匹配，请检查平台配置!");
            }

            // 域名白名单校验（Chat 类型豁免）
            if (platform.AccessType != AccessType.Chat && !string.IsNullOrEmpty(request.RequestDomain))
            {
                if (!IsDomainAllowed(platform, request.RequestDomain))
                {
                    _violationService.Log(platform.Id, platform.PlatformKey, platform.OwnerId,
                        request.RequestDomain, source, expectedSource,
                        "domain_mismatch", request.IpAddress, request.UserAgent);

                    _logger.LogWarning("域名不匹配: PlatformKey={PlatformKey}, Domain={Domain}",
                        platform.PlatformKey, request.RequestDomain);

                    return ResultData.No("接入域名未授权，请在平台设置中添加域名白名单!");
                }
            }

            return null; // 校验通过
        }

        /// <summary>
        /// 检查域名是否在白名单中
        /// </summary>
        private bool IsDomainAllowed(ChatPlatform platform, string requestDomain)
        {
            // 如果没有配置白名单，允许所有域名
            if (string.IsNullOrEmpty(platform.AllowedDomains) && string.IsNullOrEmpty(platform.Domain))
                return true;

            requestDomain = requestDomain.ToLower().Trim();

            // 优先检查 AllowedDomains（JSON 数组）
            if (!string.IsNullOrEmpty(platform.AllowedDomains))
            {
                try
                {
                    var domains = System.Text.Json.JsonSerializer.Deserialize<string[]>(platform.AllowedDomains);
                    if (domains != null && domains.Any(d => d.ToLower().Trim() == requestDomain))
                        return true;
                }
                catch
                {
                    // JSON 解析失败，忽略
                }
            }

            // 兼容旧 Domain 字段
            if (!string.IsNullOrEmpty(platform.Domain))
            {
                if (platform.Domain.ToLower().Trim() == requestDomain)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 确保分配的客服已注册到 OpenIM，如未注册则自动生成 ID 并注册
        /// </summary>
        private async Task<string> EnsureAgentRegisteredAsync(AgentAllocationResult allocation)
        {
            if (!allocation.Success || !allocation.AgentId.HasValue)
                return allocation.OpenImUserId;

            // 已有 OpenImUserId，直接返回
            if (!string.IsNullOrEmpty(allocation.OpenImUserId))
                return allocation.OpenImUserId;

            // 查询 Member 信息
            var member = _fsql.Select<Member>()
                .Where(c => c.Id == allocation.AgentId.Value)
                .First();

            if (member == null) return null;

            // 生成并注册
            var imUserId = OpenImIdGenerator.GenerateAgentId(member.Id);
            await _openImUserService.RegisterUserAsync(
                imUserId, member.Name ?? member.Username, member.Avatar ?? "");

            // 更新数据库
            member.OpenImUserId = imUserId;
            _fsql.Update<Member>().SetSource(member)
                .UpdateColumns(c => c.OpenImUserId)
                .ExecuteAffrows();

            // 同步更新分配结果
            allocation.OpenImUserId = imUserId;

            _logger.LogInformation("客服 OpenIM 自动注册: MemberId={MemberId}, ImUserId={ImUserId}",
                member.Id, imUserId);

            return imUserId;
        }

        private void RecordLogin(ChatUser chatUser, VisitorAuthRequest request)
        {
            _fsql.Insert(new ChatUserLoginRecord
            {
                ChatUserId = chatUser.Id,
                PlatformId = chatUser.PlatformId,
                ClientId = request.ClientId,
                DeviceType = request.DeviceType,
                UserAgent = request.UserAgent,
                IpAddress = request.IpAddress,
                LoginTime = DateTime.Now,
                IsDeleted = false
            }).ExecuteAffrows();
        }

        #endregion
    }
}
