using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 访客排队记录
    /// </summary>
    [Table(Name = "SB_VisitorQueue")]
    public class VisitorQueue : IEntity
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>OpenIM 聊天用户ID</summary>
        [Column(StringLength = 64)]
        public string ChatUserId { get; set; }

        /// <summary>平台ID</summary>
        public int PlatformId { get; set; }

        /// <summary>站长ID</summary>
        public int OwnerId { get; set; }

        /// <summary>排队位置</summary>
        public int QueuePosition { get; set; }

        /// <summary>入队时间</summary>
        public DateTime EnqueueTime { get; set; }

        /// <summary>状态: 0=等待 1=已分配 2=已取消 3=已超时</summary>
        public int Status { get; set; }

        /// <summary>分配的客服ID</summary>
        public int? AssignedAgentId { get; set; }

        /// <summary>分配时间</summary>
        public DateTime? AssignedTime { get; set; }

        /// <summary>取消时间</summary>
        public DateTime? CancelTime { get; set; }
    }

    /// <summary>
    /// 排队状态枚举
    /// </summary>
    public enum QueueStatus
    {
        Waiting = 0,
        Assigned = 1,
        Cancelled = 2,
        Timeout = 3
    }
}
