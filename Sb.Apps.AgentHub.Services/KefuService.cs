using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using Sb.Framework.Chat.Realtime.Conversations;
using Sb.Framework.Chat.Realtime.Presence;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 客服工作台服务
    /// </summary>
    [AutoDependencyInjection]
    public class KefuService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<KefuService> _logger;
        private readonly QueueService _queueService;
        private readonly IPresenceService _presence;
        private readonly IConversationService _conversations;
        private readonly CustomerServiceConfigService _configService;
        private readonly MemberService _memberService;
        private readonly ChatUserService _chatUserService;

        public KefuService(IHttpContextAccessor httpContextAccessor,
            ILogger<KefuService> logger, QueueService queueService,
            IPresenceService presence, IConversationService conversations,
            CustomerServiceConfigService configService,
            MemberService memberService, ChatUserService chatUserService)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _queueService = queueService;
            _presence = presence;
            _conversations = conversations;
            _configService = configService;
            _memberService = memberService;
            _chatUserService = chatUserService;
        }

        /// <summary>
        /// 获取当前客服的访客列表
        /// </summary>
        public async Task<ResultData> GetMyVisitors(string keyword = "", int page = 1, int pageSize = 50)
        {
            int agentId = _httpContextAccessor.HttpContext!.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            // 获取 ownerId 和广播配置
            var member = _memberService.GetById(agentId);
            int ownerId = member?.Role == MemberRole.Owner ? member.Id : (member?.OwnerId ?? 0);
            var csConfig = _configService.GetEffectiveConfig(ownerId);

            var (dbData, dataCount) = _chatUserService.GetAgentVisitors(
                agentId, ownerId, csConfig.EnableBroadcast, keyword, page, pageSize);

            // 通过 Realtime 在线状态服务查询真实在线状态
            var chatUserIds = dbData.Select(d => d.ChatUserId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            var onlineMap = _presence.IsOnline(chatUserIds);

            var data = dbData.Select(d => new
            {
                d.Id, d.ChatUserId, d.Nickname, d.Avatar, d.NicknameRemark,
                OnlineStatus = onlineMap.TryGetValue(d.ChatUserId, out var isOnline) && isOnline
                    ? OnlineStatus.Online : OnlineStatus.Offline,
                d.LastActiveTime, d.UserType, d.Remark, d.GroupId
            }).ToList();

            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 更新客服在线状态
        /// </summary>
        public ResultData UpdateAgentStatus(OnlineStatus status)
        {
            int agentId = _httpContextAccessor.HttpContext!.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var member = _memberService.GetById(agentId);
            if (member == null) return ResultData.No("用户不存在!");

            _memberService.UpdateOnlineStatus(agentId, status, DateTime.Now);

            _logger.LogInformation("客服状态更新: AgentId={AgentId}, Status={Status}", agentId, status);

            // 广播模式下：上线/下线自动管理群组成员
            int ownerId = member.Role == MemberRole.Owner ? member.Id : (member.OwnerId ?? 0);
            var csConfig = _configService.GetEffectiveConfig(ownerId);
            if (csConfig.EnableBroadcast && !string.IsNullOrEmpty(member.RealtimeUserId))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ManageBroadcastGroupMembershipAsync(member, ownerId, status != OnlineStatus.Offline);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "广播模式群组成员管理异常: AgentId={AgentId}", agentId);
                    }
                });
            }

            return ResultData.Ok("状态更新成功!", new { status });
        }

        /// <summary>
        /// 广播模式：客服上线时加入所有活跃群组，下线时移出（主客服除外）
        /// </summary>
        private async Task ManageBroadcastGroupMembershipAsync(Member agent, int ownerId, bool isOnline)
        {
            // 查询该 owner 下所有有 GroupId 的活跃访客
            var activeChats = _chatUserService.GetActiveChatsWithGroupId(ownerId);

            if (activeChats.Count == 0) return;

            foreach (var chat in activeChats)
            {
                // 主客服始终在群里，不移出
                if (!isOnline && chat.AssignedAgentId == agent.Id)
                    continue;

                if (isOnline)
                {
                    await _conversations.AddMemberAsync(long.Parse(chat.GroupId), agent.RealtimeUserId);
                }
                else
                {
                    await _conversations.RemoveMemberAsync(long.Parse(chat.GroupId), agent.RealtimeUserId);
                }
            }

            _logger.LogInformation("广播模式群组成员管理完成: AgentId={AgentId}, Online={Online}, Groups={Count}",
                agent.Id, isOnline, activeChats.Count);
        }

        /// <summary>
        /// 转接访客给其他客服
        /// </summary>
        public ResultData TransferVisitor(int chatUserId, int targetAgentId)
        {
            int currentAgentId = _httpContextAccessor.HttpContext!.User.Id();
            if (currentAgentId <= 0) return ResultData.No("授权失效!");

            var chatUser = _chatUserService.GetById(chatUserId);
            if (chatUser == null) return ResultData.No("用户不存在!");

            if (chatUser.AssignedAgentId != currentAgentId)
                return ResultData.No("该用户不在您的服务列表中!");

            var targetAgent = _memberService.GetActiveAgent(targetAgentId);
            if (targetAgent == null) return ResultData.No("目标客服不存在或不可用!");

            // 执行转接
            _chatUserService.UpdateAssignedAgent(chatUserId, targetAgentId);

            // 更新双方服务计数
            _memberService.DecrementCurrentCount(currentAgentId);
            _memberService.IncrementCurrentCount(targetAgentId);

            _logger.LogInformation("访客转接: ChatUserId={ChatUserId}, From={From}, To={To}",
                chatUserId, currentAgentId, targetAgentId);

            return ResultData.Ok("转接成功!");
        }

        /// <summary>
        /// 获取可转接的客服列表
        /// </summary>
        public ResultData GetTransferableAgents()
        {
            var user = _httpContextAccessor.HttpContext!.User;
            int currentAgentId = user.Id();
            string role = user.Role();

            int ownerId;
            if (role == "Owner")
            {
                ownerId = currentAgentId;
            }
            else
            {
                var member = _memberService.GetById(currentAgentId);
                ownerId = member?.OwnerId ?? 0;
            }

            if (ownerId <= 0) return ResultData.No("授权失效!");

            var agents = _memberService.GetTransferableAgents(ownerId, currentAgentId)
                .Select(c => new
                {
                    c.Id, c.Username, c.Name, c.Avatar,
                    c.OnlineStatus, c.CurrentCount, c.MaxConcurrent
                });

            return ResultData.Ok(agents);
        }

        /// <summary>
        /// 获取当前客服状态
        /// </summary>
        public ResultData GetMyStatus()
        {
            int agentId = _httpContextAccessor.HttpContext!.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var member = _memberService.GetById(agentId);
            if (member == null) return ResultData.No("用户不存在!");

            int visitorCount = _chatUserService.CountAssignedByAgent(agentId);

            // 排队等待人数
            int ownerId = member.Role == MemberRole.Owner ? member.Id : (member.OwnerId ?? 0);
            int queueWaiting = _queueService.GetWaitingCount(ownerId);

            return ResultData.Ok(new
            {
                member.OnlineStatus,
                member.MaxConcurrent,
                member.CurrentCount,
                visitorCount,
                queueWaiting
            });
        }

        /// <summary>
        /// 获取排队等待列表
        /// </summary>
        public ResultData GetQueueWaitingList()
        {
            int agentId = _httpContextAccessor.HttpContext!.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var member = _memberService.GetById(agentId);
            if (member == null) return ResultData.No("用户不存在!");

            int ownerId = member.Role == MemberRole.Owner ? member.Id : (member.OwnerId ?? 0);
            var waitingList = _queueService.GetWaitingList(ownerId, 20);

            // 关联 ChatUser 信息
            var chatUserIds = waitingList.Select(q => q.ChatUserId).ToList();
            var chatUsers = chatUserIds.Count > 0
                ? _chatUserService.GetByChatUserIds(chatUserIds)
                : new List<ChatUser>();

            var result = waitingList.Select(q =>
            {
                var cu = chatUsers.FirstOrDefault(c => c.ChatUserId == q.ChatUserId);
                return new
                {
                    q.Id,
                    q.ChatUserId,
                    Nickname = cu?.NicknameRemark ?? cu?.Nickname ?? "匿名",
                    Avatar = cu?.Avatar ?? "",
                    q.QueuePosition,
                    q.EnqueueTime,
                    WaitMinutes = (int)(DateTime.Now - q.EnqueueTime).TotalMinutes
                };
            }).ToList();

            return ResultData.Ok(result);
        }

        /// <summary>
        /// 从排队中接入访客
        /// </summary>
        public ResultData AcceptFromQueue(string chatUserId)
        {
            int agentId = _httpContextAccessor.HttpContext!.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var member = _memberService.GetById(agentId);
            if (member == null) return ResultData.No("用户不存在!");

            // 校验并发上限
            if (member.MaxConcurrent > 0 && member.CurrentCount >= member.MaxConcurrent)
                return ResultData.No("已达最大并发数!");

            // 查找排队记录并标记为已分配
            var queueItem = _queueService.GetWaitingByChatUserId(chatUserId);
            if (queueItem == null) return ResultData.No("该访客不在排队中!");
            _queueService.MarkAssigned(queueItem.Id, agentId);

            // 获取或创建 ChatUser 并分配客服
            var chatUser = _chatUserService.GetByChatUserId(chatUserId);
            if (chatUser != null)
            {
                _chatUserService.UpdateAssignedAgent(chatUser.Id, agentId);
            }

            // 增加客服计数
            _memberService.IncrementCurrentCount(agentId);

            _logger.LogInformation("客服接入排队访客: AgentId={AgentId}, ChatUserId={ChatUserId}", agentId, chatUserId);

            return ResultData.Ok("接入成功!");
        }
    }
}
