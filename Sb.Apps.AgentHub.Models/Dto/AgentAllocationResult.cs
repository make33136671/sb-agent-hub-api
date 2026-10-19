namespace Sb.Apps.AgentHub.Models.Dto
{
    /// <summary>
    /// 客服分配结果
    /// </summary>
    public class AgentAllocationResult
    {
        /// <summary>
        /// 是否分配成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 客服ID
        /// </summary>
        public int? AgentId { get; set; }
        /// <summary>
        /// 客服名称
        /// </summary>
        public string AgentName { get; set; } = null!;
        /// <summary>
        /// 客服头像
        /// </summary>
        public string AgentAvatar { get; set; } = null!;
        /// <summary>
        /// 客服用户ID
        /// </summary>
        public string AgentUserId { get; set; } = null!;
        /// <summary>
        /// Realtime 用户ID
        /// </summary>
        public string RealtimeUserId { get; set; } = null!;
        /// <summary>
        /// 欢迎消息
        /// </summary>
        public string WelcomeMessage { get; set; } = null!;
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// 分配到的客服是否在线
        /// </summary>
        public bool AgentOnline { get; set; }

        /// <summary>
        /// 是否需要排队（并发会话超限时为 true）
        /// </summary>
        public bool NeedsQueue { get; set; }
    }
}
