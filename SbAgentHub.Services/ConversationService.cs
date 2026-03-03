using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;
using SbAgentHub.Models.Dto;
using static SbAgentHub.Models.Dto.SystemMessageTypes;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 会话管理服务 — Admin 后台会话列表
    /// </summary>
    [AutoDependencyInjection]
    public class ConversationService
    {
        private readonly IFreeSql _fsql;
        private readonly ILogger<ConversationService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OpenImConversationService _openImConversationService;

        public ConversationService(
            IFreeSql fsql,
            ILogger<ConversationService> logger,
            IHttpContextAccessor httpContextAccessor,
            OpenImConversationService openImConversationService)
        {
            _fsql = fsql;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _openImConversationService = openImConversationService;
        }

        /// <summary>
        /// 获取会话列表
        /// </summary>
        public async Task<ResultData> GetListAsync(string keyword = "", int? agentId = null,
            int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext.User;
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
                    AgentImUserId = m.OpenImUserId,
                    c.FirstVisitTime,
                    c.LastActiveTime
                });

            if (!chatUsers.Any())
            {
                return ResultPage.Ok(new List<ConversationListItem>(), 0, page, pageSize, "数据查询成功");
            }

            // 3. 收集客服 IM UserID，批量获取 OpenIM 会话信息
            var agentImUserIds = chatUsers
                .Where(c => !string.IsNullOrEmpty(c.AgentImUserId))
                .Select(c => c.AgentImUserId)
                .Distinct()
                .ToList();

            // 按客服获取会话数据
            var conversationMap = new Dictionary<string, OpenImConversationElem>();
            foreach (var agentImUserId in agentImUserIds)
            {
                var sortedResult = await _openImConversationService.GetSortedConversationsAsync(
                    agentImUserId, 1, 100);

                if (sortedResult?.ConversationElems != null)
                {
                    foreach (var elem in sortedResult.ConversationElems)
                    {
                        // 会话 key: agentImUserId + visitorChatUserId
                        if (!string.IsNullOrEmpty(elem.UserID))
                        {
                            var key = $"{agentImUserId}_{elem.UserID}";
                            conversationMap[key] = elem;
                        }
                    }
                }
            }

            // 4. 合并数据
            var items = chatUsers.Select(cu =>
            {
                var key = $"{cu.AgentImUserId}_{cu.ChatUserId}";
                conversationMap.TryGetValue(key, out var convElem);

                string lastMsgContent = null;
                long? lastMsgTime = null;
                string lastMsgSenderId = null;
                int unreadCount = 0;
                string conversationId = null;

                if (convElem != null)
                {
                    conversationId = convElem.ConversationID;
                    unreadCount = convElem.UnreadCount;

                    if (convElem.MsgInfo != null)
                    {
                        lastMsgContent = ParseMsgContent(convElem.MsgInfo.ContentType, convElem.MsgInfo.Content);
                        lastMsgTime = convElem.MsgInfo.SendTime;
                        lastMsgSenderId = convElem.MsgInfo.SendID;
                    }
                }

                return new ConversationListItem
                {
                    Id = cu.Id,
                    VisitorNickname = cu.NicknameRemark ?? cu.Nickname,
                    VisitorAvatar = cu.Avatar,
                    VisitorChatUserId = cu.ChatUserId,
                    VisitorOnlineStatus = (int)cu.OnlineStatus,
                    AgentId = cu.AssignedAgentId,
                    AgentName = cu.AgentName,
                    AgentImUserId = cu.AgentImUserId,
                    ConversationId = conversationId,
                    LastMsgContent = lastMsgContent,
                    LastMsgTime = lastMsgTime,
                    LastMsgSenderId = lastMsgSenderId,
                    UnreadCount = unreadCount,
                    FirstVisitTime = cu.FirstVisitTime,
                    LastActiveTime = cu.LastActiveTime
                };
            }).ToList();

            return ResultPage.Ok(items, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 解析消息内容（不同 contentType 展示不同）
        /// </summary>
        private string ParseMsgContent(int contentType, string content)
        {
            if (string.IsNullOrEmpty(content)) return null;

            return contentType switch
            {
                101 => TryExtractText(content),         // 文本
                102 => "[图片]",
                103 => "[语音]",
                104 => "[视频]",
                105 => "[文件]",
                106 => "[位置]",
                110 => ParseCustomMessage(content),     // 自定义消息（含系统消息）
                _ => "[消息]"
            };
        }

        /// <summary>
        /// 尝试从 JSON 内容中提取文本
        /// </summary>
        private string TryExtractText(string content)
        {
            try
            {
                // OpenIM 文本消息格式: {"content":"xxx"} 或直接是文本
                if (content.StartsWith("{"))
                {
                    var doc = System.Text.Json.JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("content", out var textElem))
                    {
                        return textElem.GetString();
                    }
                }
                return content;
            }
            catch
            {
                return content;
            }
        }

        /// <summary>
        /// 解析自定义消息（contentType=110），识别系统消息类型
        /// 与 openim-core-js SystemMessage.ts 中 SYSTEM_MESSAGE_TYPES 对齐
        /// </summary>
        private string ParseCustomMessage(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content)) return "[自定义消息]";

                // 自定义消息的 data 字段是 JSON 字符串
                var doc = System.Text.Json.JsonDocument.Parse(content);

                // 先尝试读取 data 字段（OpenIM 自定义消息结构: {data, extension, description}）
                string dataStr = content;
                if (doc.RootElement.TryGetProperty("data", out var dataElem))
                {
                    dataStr = dataElem.GetString() ?? content;
                }

                // 尝试解析 data 中的 type 字段
                var dataDoc = System.Text.Json.JsonDocument.Parse(dataStr);
                if (dataDoc.RootElement.TryGetProperty("type", out var typeElem))
                {
                    var msgType = typeElem.GetString();
                    // 系统消息类型（与 SystemMessageTypes / core-js SYSTEM_MESSAGE_TYPES 对齐）
                    if (Array.Exists(SystemMessageTypes.All, t => t == msgType))
                        return null; // 系统消息不显示
                    return msgType switch
                    {
                        "image" => "[图片]",
                        _ => "[自定义消息]"
                    };
                }

                // 没有 type 字段，尝试读 description
                if (doc.RootElement.TryGetProperty("description", out var descElem))
                {
                    var desc = descElem.GetString();
                    if (!string.IsNullOrEmpty(desc)) return desc;
                }

                return "[自定义消息]";
            }
            catch
            {
                return "[自定义消息]";
            }
        }
    }
}
