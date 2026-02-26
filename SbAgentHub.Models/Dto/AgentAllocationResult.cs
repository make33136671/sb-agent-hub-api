namespace SbAgentHub.Models.Dto
{
    /// <summary>
    /// 客服分配结果
    /// </summary>
    public class AgentAllocationResult
    {
        public bool Success { get; set; }
        public int? AgentId { get; set; }
        public string AgentImUserId { get; set; }
        public string AgentName { get; set; }
        public string WelcomeMessage { get; set; }
        public string FailReason { get; set; }
    }
}
