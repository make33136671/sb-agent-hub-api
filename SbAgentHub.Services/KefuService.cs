using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 客服工作台服务
    /// </summary>
    [AutoDependencyInjection]
    public class KefuService
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<KefuService> _logger;
        private readonly QueueService _queueService;
        private readonly OpenImUserService _openImUserService;

        public KefuService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor,
            ILogger<KefuService> logger, QueueService queueService, OpenImUserService openImUserService)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _queueService = queueService;
            _openImUserService = openImUserService;
        }

        /// <summary>
        /// 获取当前客服的访客列表
        /// </summary>
        public async Task<ResultData> GetMyVisitors(string keyword = "", int page = 1, int pageSize = 50)
        {
            int agentId = _httpContextAccessor.HttpContext.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var query = _fsql.Select<ChatUser>()
                .Where(c => c.AssignedAgentId == agentId && !c.IsDeleted);

            query = query.WhereIf(!string.IsNullOrEmpty(keyword),
                c => c.Nickname.Contains(keyword) || c.NicknameRemark.Contains(keyword) || c.ChatUserId.Contains(keyword));

            var dbData = query.OrderByDescending(c => c.LastActiveTime)
                .Count(out long dataCount)
                .Page(page, pageSize)
                .ToList(c => new
                {
                    c.Id, c.ChatUserId, c.Nickname, c.Avatar, c.NicknameRemark,
                    c.OnlineStatus, c.LastActiveTime, c.UserType, c.Remark
                });

            // 通过 OpenIM 查询真实在线状态
            var chatUserIds = dbData.Select(d => d.ChatUserId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            var onlineMap = await _openImUserService.GetUsersOnlineStatusAsync(chatUserIds);

            var data = dbData.Select(d => new
            {
                d.Id, d.ChatUserId, d.Nickname, d.Avatar, d.NicknameRemark,
                OnlineStatus = onlineMap.TryGetValue(d.ChatUserId, out var isOnline) && isOnline
                    ? OnlineStatus.Online : OnlineStatus.Offline,
                d.LastActiveTime, d.UserType, d.Remark
            }).ToList();

            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 更新客服在线状态
        /// </summary>
        public ResultData UpdateAgentStatus(OnlineStatus status)
        {
            int agentId = _httpContextAccessor.HttpContext.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            _fsql.Update<Member>()
                .Where(c => c.Id == agentId)
                .Set(c => c.OnlineStatus, status)
                .Set(c => c.LastOnlineTime, DateTime.Now)
                .ExecuteAffrows();

            _logger.LogInformation("客服状态更新: AgentId={AgentId}, Status={Status}", agentId, status);

            return ResultData.Ok("状态更新成功!", new { status });
        }

        /// <summary>
        /// 转接访客给其他客服
        /// </summary>
        public ResultData TransferVisitor(int chatUserId, int targetAgentId)
        {
            int currentAgentId = _httpContextAccessor.HttpContext.User.Id();
            if (currentAgentId <= 0) return ResultData.No("授权失效!");

            var chatUser = _fsql.Select<ChatUser>().Where(c => c.Id == chatUserId).First();
            if (chatUser == null) return ResultData.No("用户不存在!");

            if (chatUser.AssignedAgentId != currentAgentId)
                return ResultData.No("该用户不在您的服务列表中!");

            var targetAgent = _fsql.Select<Member>()
                .Where(c => c.Id == targetAgentId && c.Role == MemberRole.Agent && c.Status == UserStatus.Active)
                .First();

            if (targetAgent == null) return ResultData.No("目标客服不存在或不可用!");

            // 执行转接
            _fsql.Update<ChatUser>()
                .Where(c => c.Id == chatUserId)
                .Set(c => c.AssignedAgentId, targetAgentId)
                .ExecuteAffrows();

            // 更新双方服务计数
            _fsql.Update<Member>()
                .Where(c => c.Id == currentAgentId && c.CurrentCount > 0)
                .SetRaw("CurrentCount = CurrentCount - 1")
                .ExecuteAffrows();

            _fsql.Update<Member>()
                .Where(c => c.Id == targetAgentId)
                .SetRaw("CurrentCount = CurrentCount + 1")
                .ExecuteAffrows();

            _logger.LogInformation("访客转接: ChatUserId={ChatUserId}, From={From}, To={To}",
                chatUserId, currentAgentId, targetAgentId);

            return ResultData.Ok("转接成功!");
        }

        /// <summary>
        /// 获取可转接的客服列表
        /// </summary>
        public ResultData GetTransferableAgents()
        {
            var user = _httpContextAccessor.HttpContext.User;
            int currentAgentId = user.Id();
            string role = user.Role();

            int ownerId;
            if (role == "Owner")
            {
                ownerId = currentAgentId;
            }
            else
            {
                var member = _fsql.Select<Member>().Where(c => c.Id == currentAgentId).First();
                ownerId = member?.OwnerId ?? 0;
            }

            if (ownerId <= 0) return ResultData.No("授权失效!");

            var agents = _fsql.Select<Member>()
                .Where(c => c.Id != currentAgentId
                    && (c.OwnerId == ownerId || c.Id == ownerId)
                    && c.Status == UserStatus.Active
                    && c.OnlineStatus != OnlineStatus.Offline)
                .ToList(c => new
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
            int agentId = _httpContextAccessor.HttpContext.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var member = _fsql.Select<Member>().Where(c => c.Id == agentId).First();
            if (member == null) return ResultData.No("用户不存在!");

            int visitorCount = (int)_fsql.Select<ChatUser>()
                .Where(c => c.AssignedAgentId == agentId && !c.IsDeleted)
                .Count();

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
            int agentId = _httpContextAccessor.HttpContext.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var member = _fsql.Select<Member>().Where(c => c.Id == agentId).First();
            if (member == null) return ResultData.No("用户不存在!");

            int ownerId = member.Role == MemberRole.Owner ? member.Id : (member.OwnerId ?? 0);
            var waitingList = _queueService.GetWaitingList(ownerId, 20);

            // 关联 ChatUser 信息
            var chatUserIds = waitingList.Select(q => q.ChatUserId).ToList();
            var chatUsers = chatUserIds.Count > 0
                ? _fsql.Select<ChatUser>().Where(c => chatUserIds.Contains(c.ChatUserId)).ToList()
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
            int agentId = _httpContextAccessor.HttpContext.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var member = _fsql.Select<Member>().Where(c => c.Id == agentId).First();
            if (member == null) return ResultData.No("用户不存在!");

            // 校验并发上限
            if (member.MaxConcurrent > 0 && member.CurrentCount >= member.MaxConcurrent)
                return ResultData.No("已达最大并发数!");

            // 查找排队记录并标记为已分配
            var queueItem = _fsql.Select<VisitorQueue>()
                .Where(q => q.ChatUserId == chatUserId && q.Status == (int)QueueStatus.Waiting)
                .First();
            if (queueItem == null) return ResultData.No("该访客不在排队中!");
            _queueService.MarkAssigned(queueItem.Id, agentId);

            // 获取或创建 ChatUser 并分配客服
            var chatUser = _fsql.Select<ChatUser>().Where(c => c.ChatUserId == chatUserId).First();
            if (chatUser != null)
            {
                _fsql.Update<ChatUser>()
                    .Where(c => c.Id == chatUser.Id)
                    .Set(c => c.AssignedAgentId, agentId)
                    .ExecuteAffrows();
            }

            // 增加客服计数
            _fsql.Update<Member>()
                .Where(c => c.Id == agentId)
                .SetRaw("CurrentCount = CurrentCount + 1")
                .ExecuteAffrows();

            _logger.LogInformation("客服接入排队访客: AgentId={AgentId}, ChatUserId={ChatUserId}", agentId, chatUserId);

            return ResultData.Ok("接入成功!");
        }
    }
}
