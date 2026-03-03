namespace SbAgentHub.Models.Dto
{
    /// <summary>
    /// 会话列表请求
    /// </summary>
    public class ConversationListRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Keyword { get; set; }
        public int? AgentId { get; set; }
    }

    /// <summary>
    /// 会话列表项
    /// </summary>
    public class ConversationListItem
    {
        public int Id { get; set; }
        public string VisitorNickname { get; set; }
        public string VisitorAvatar { get; set; }
        public string VisitorChatUserId { get; set; }
        public int VisitorOnlineStatus { get; set; }
        public int? AgentId { get; set; }
        public string AgentName { get; set; }
        public string AgentImUserId { get; set; }
        public string ConversationId { get; set; }
        public string LastMsgContent { get; set; }
        public long? LastMsgTime { get; set; }
        public string LastMsgSenderId { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? FirstVisitTime { get; set; }
        public DateTime? LastActiveTime { get; set; }
    }
}
