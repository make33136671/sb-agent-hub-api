namespace Sb.Apps.AgentHub.Models.Dto
{
    /// <summary>
    /// 会话列表请求
    /// </summary>
    public class ConversationListRequest
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; } = 1;
        /// <summary>
        /// 每页条数
        /// </summary>
        public int PageSize { get; set; } = 20;
        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string Keyword { get; set; } = null!;
        /// <summary>
        /// 客服ID
        /// </summary>
        public int? AgentId { get; set; }
    }

    /// <summary>
    /// 会话列表项
    /// </summary>
    public class ConversationListItem
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 访客昵称
        /// </summary>
        public string VisitorNickname { get; set; } = null!;
        /// <summary>
        /// 访客头像
        /// </summary>
        public string VisitorAvatar { get; set; } = null!;
        /// <summary>
        /// 访客聊天用户ID
        /// </summary>
        public string VisitorChatUserId { get; set; } = null!;
        /// <summary>
        /// 访客在线状态
        /// </summary>
        public int VisitorOnlineStatus { get; set; }
        /// <summary>
        /// 客服ID
        /// </summary>
        public int? AgentId { get; set; }
        /// <summary>
        /// 客服名称
        /// </summary>
        public string AgentName { get; set; } = null!;
        /// <summary>
        /// 客服用户ID
        /// </summary>
        public string AgentUserId { get; set; } = null!;
        /// <summary>
        /// 会话ID
        /// </summary>
        public string ConversationId { get; set; } = null!;
        /// <summary>
        /// 最后一条消息内容
        /// </summary>
        public string LastMsgContent { get; set; } = null!;
        /// <summary>
        /// 最后一条消息时间
        /// </summary>
        public long? LastMsgTime { get; set; }
        /// <summary>
        /// 最后一条消息发送者ID
        /// </summary>
        public string LastMsgSenderId { get; set; } = null!;
        /// <summary>
        /// 未读消息数
        /// </summary>
        public int UnreadCount { get; set; }
        /// <summary>
        /// 首次访问时间
        /// </summary>
        public DateTime? FirstVisitTime { get; set; }
        /// <summary>
        /// 最后活跃时间
        /// </summary>
        public DateTime? LastActiveTime { get; set; }
    }
}
