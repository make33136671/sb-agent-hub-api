using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Models.Dto;
using Sb.Framework.Chat.Realtime.Conversations;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 会话管理服务 — Admin 后台会话列表（数据来自 Realtime 模块）
    /// </summary>
    [AutoDependencyInjection]
    public class ConversationService
    {
        private readonly IFreeSql _fsql;
        private readonly ILogger<ConversationService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConversationService _conversations;

        public ConversationService(
            IFreeSql fsql,
            ILogger<ConversationService> logger,
            IHttpContextAccessor httpContextAccessor,
            IConversationService conversations)
        {
            _fsql = fsql;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _conversations = conversations;
        }

        /// <summary>
        /// 获取会话列表
        /// </summary>
        public async Task<ResultData> GetListAsync(string keyword = "", int? agentId = null,
            int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            string currentUserType = user.UserType();
            string role = user.Role();
            int currentUserId = user.Id();

            // 1. 查询有分配客服的访客列表
            var query = _fsql.Select<ChatUser, Member>()
                .LeftJoin((c, m) => c.AssignedAgentId == m.Id)
                .Where((c, m) => c.AssignedAgentId != null && c.AssignedAgentId > 0);

            // 根据角色限制数据范围
            if (currentUserType == "Member" && role == "Owner")
            {
                query = query.Where((c, m) => c.OwnerId == currentUserId);
            }
            else if (currentUserType == "Member" && role == "Agent")
            {
                query = query.Where((c, m) => c.AssignedAgentId == currentUserId);
            }

            // 关键词搜索
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where((c, m) =>
                    c.Nickname.Contains(keyword) || c.NicknameRemark.Contains(keyword));
            }

            // 客服筛选
            if (agentId.HasValue)
            {
                query = query.Where((c, m) => c.AssignedAgentId == agentId.Value);
            }

            // 2. 分页查询
            var chatUsers = query
                .OrderByDescending((c, m) => c.LastActiveTime)
                .Count(out long dataCount)
                .Page(page, pageSize)
                .ToList((c, m) => new
                {
                    c.Id,
                    c.ChatUserId,
                    c.Nickname,
                    c.NicknameRemark,
                    c.Avatar,
                    c.OnlineStatus,
                    c.AssignedAgentId,
                    AgentName = m.Name ?? m.Username,
                    AgentUserId = m.RealtimeUserId,
                    c.GroupId,
                    c.FirstVisitTime,
                    c.LastActiveTime
                });

            if (!chatUsers.Any())
            {
                return ResultPage.Ok(new List<ConversationListItem>(), 0, page, pageSize, "数据查询成功");
            }

            // 3. 逐个访客会话：用 GroupId（=chat_rt_conversation.id）按客服查未读/最后消息
            var items = new List<ConversationListItem>();
            foreach (var cu in chatUsers)
            {
                RealtimeConversation? conv = null;
                if (!string.IsNullOrEmpty(cu.GroupId) && long.TryParse(cu.GroupId, out var convId))
                {
                    conv = await _conversations.GetAsync(convId, cu.AgentUserId);
                }

                items.Add(new ConversationListItem
                {
                    Id = cu.Id,
                    VisitorNickname = cu.NicknameRemark ?? cu.Nickname,
                    VisitorAvatar = cu.Avatar,
                    VisitorChatUserId = cu.ChatUserId,
                    VisitorOnlineStatus = (int)cu.OnlineStatus,
                    AgentId = cu.AssignedAgentId,
                    AgentName = cu.AgentName,
                    AgentUserId = cu.AgentUserId,
                    ConversationId = conv?.Id.ToString() ?? "",
                    LastMsgContent = conv?.LastMsgPreview ?? "",
                    LastMsgTime = null,
                    LastMsgSenderId = "",
                    UnreadCount = (int)(conv?.UnreadCount ?? 0),
                    FirstVisitTime = cu.FirstVisitTime,
                    LastActiveTime = cu.LastActiveTime
                });
            }

            return ResultPage.Ok(items, dataCount, page, pageSize, "数据查询成功");
        }
    }
}

