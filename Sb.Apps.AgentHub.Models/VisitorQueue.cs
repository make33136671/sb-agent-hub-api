using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 访客排队记录
    /// </summary>
    [Table(Name = "sb_agenthub_visitor_queue")]
    [Index("ix_visitorqueue_chatuserid", "ChatUserId")]
    [Index("ix_visitorqueue_platform_owner", "PlatformId,OwnerId")]
    [Index("ix_visitorqueue_status", "Status")]
    public class VisitorQueue : IEntity
    {
        /// <summary>主键Id</summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>Realtime 聊天用户ID</summary>
        [Column(StringLength = 64)]
        public string ChatUserId { get; set; } = null!;

        /// <summary>平台ID</summary>
        public int PlatformId { get; set; }

        /// <summary>站长ID</summary>
        public int OwnerId { get; set; }

        /// <summary>排队位置</summary>
        public int QueuePosition { get; set; }

        /// <summary>入队时间</summary>
        public DateTime EnqueueTime { get; set; }

        /// <summary>状态: Waiting=等待 Assigned=已分配 Cancelled=已取消 Timeout=已超时</summary>
        [Column(Name = "Status", MapType = typeof(string))]
        public QueueStatus Status { get; set; }

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
        /// <summary>等待中</summary>
        Waiting = 0,
        /// <summary>已分配</summary>
        Assigned = 1,
        /// <summary>已取消</summary>
        Cancelled = 2,
        /// <summary>已超时</summary>
        Timeout = 3
    }
}
