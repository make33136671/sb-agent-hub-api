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

        public KefuService(IFreeSql fsql, IHttpContextAccessor httpContextAccessor, ILogger<KefuService> logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前客服的访客列表
        /// </summary>
        public ResultData GetMyVisitors(string keyword = "", int page = 1, int pageSize = 50)
        {
            int agentId = _httpContextAccessor.HttpContext.User.Id();
            if (agentId <= 0) return ResultData.No("授权失效!");

            var query = _fsql.Select<ChatUser>()
                .Where(c => c.AssignedAgentId == agentId && !c.IsDeleted);

            query = query.WhereIf(!string.IsNullOrEmpty(keyword),
                c => c.Nickname.Contains(keyword) || c.NicknameRemark.Contains(keyword) || c.ChatUserId.Contains(keyword));

            var data = query.OrderByDescending(c => c.LastActiveTime)
                .Count(out long dataCount)
                .Page(page, pageSize)
                .ToList(c => new
                {
                    c.Id, c.ChatUserId, c.Nickname, c.Avatar, c.NicknameRemark,
                    c.OnlineStatus, c.LastActiveTime, c.UserType, c.Remark
                });

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

            return ResultData.Ok(new
            {
                member.OnlineStatus,
                member.MaxConcurrent,
                member.CurrentCount,
                visitorCount
            });
        }
    }
}
