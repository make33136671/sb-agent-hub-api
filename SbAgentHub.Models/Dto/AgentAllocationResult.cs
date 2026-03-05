namespace SbAgentHub.Models.Dto
{
    /// <summary>
    /// 客服分配结果
    /// </summary>
    public class AgentAllocationResult
    {
        public bool Success { get; set; }
        public int? AgentId { get; set; }
        public string AgentName { get; set; }
        public string AgentAvatar { get; set; }
        public string AgentImUserId { get; set; }
        public string OpenImUserId { get; set; }
        public string WelcomeMessage { get; set; }
        public string Message { get; set; }

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
