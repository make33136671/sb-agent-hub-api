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
            var allocation = _allocationService.AllocateAgent(platform.OwnerId);

            if (allocation.Success && chatUser.AssignedAgentId != allocation.AgentId)
            {
                chatUser.AssignedAgentId = allocation.AgentId;
                _fsql.Update<ChatUser>().SetSource(chatUser)
                    .UpdateColumns(c => c.AssignedAgentId)
                    .ExecuteAffrows();
            }

            // 5. 记录登录
            RecordLogin(chatUser, request);

            // 6. 发送欢迎语
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
                AgentOnline = allocation.Success,
                OfflineMessage = allocation.Success ? null : allocation.Message,
                WelcomeMessage = allocation.WelcomeMessage,
                PlatformName = platform.PlatformName
            });
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

        #region Private

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
