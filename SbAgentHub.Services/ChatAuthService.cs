using FreeSql;
using Microsoft.Extensions.Logging;
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

        public ChatAuthService(
            IFreeSql fsql,
            ILogger<ChatAuthService> logger,
            ChatPlatformService platformService,
            ChatUserService chatUserService,
            AgentAllocationService allocationService,
            OpenImUserService openImUserService,
            OpenImAuthService openImAuthService,
            OpenImMessageService openImMessageService)
        {
            _fsql = fsql;
            _logger = logger;
            _platformService = platformService;
            _chatUserService = chatUserService;
            _allocationService = allocationService;
            _openImUserService = openImUserService;
            _openImAuthService = openImAuthService;
            _openImMessageService = openImMessageService;
        }

        /// <summary>
        /// 访客认证 — Widget 初始化时调用
        /// </summary>
        public async Task<ResultData> VisitorAuthAsync(VisitorAuthRequest request)
        {
            // 1. 验证平台
            var platform = _platformService.GetByPlatformKey(request.PlatformKey);
            if (platform == null)
                return ResultData.No("无效的平台秘钥!");

            if (!platform.IsEnabled)
                return ResultData.No("该平台已停用!");

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

            // 4. 分配客服
            var allocation = _allocationService.AllocateAgent(platform.OwnerId, platform.Id);

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
        public async Task<ResultData> GetImTokenForSdkAsync(GetImTokenFormRequest request)
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
                Features = new SdkFeatureFlags()
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
                return ResultData.No("获取 Token 失败!");

            return ResultData.Ok(new ImTokenResponse
            {
                Token = tokenData.Token,
                ExpireTimeSeconds = tokenData.ExpireTimeSeconds
            });
        }

        /// <summary>
        /// 获取客服的 OpenIM Token（管理端调用）
        /// </summary>
        public async Task<ResultData> GetAgentImTokenAsync(int memberId)
        {
            var member = _fsql.Select<Member>().Where(c => c.Id == memberId).First();
            if (member == null)
                return ResultData.No("用户不存在!");

            string imUserId = member.OpenImUserId;
            if (string.IsNullOrEmpty(imUserId))
            {
                // 自动生成并注册
                imUserId = OpenImIdGenerator.GenerateAgentId(memberId);
                await _openImUserService.RegisterUserAsync(imUserId, member.Name ?? member.Username, member.Avatar ?? "");

                member.OpenImUserId = imUserId;
                _fsql.Update<Member>().SetSource(member).UpdateColumns(c => c.OpenImUserId).ExecuteAffrows();
            }

            var tokenData = await _openImAuthService.GetUserTokenAsync(imUserId);
            if (tokenData == null)
                return ResultData.No("获取 Token 失败!");

            return ResultData.Ok(new
            {
                imUserId,
                token = tokenData.Token,
                expireTimeSeconds = tokenData.ExpireTimeSeconds
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
                return ResultData.No("获取 Token 失败!");

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
