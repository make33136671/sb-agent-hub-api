using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 客服分配配置
    /// </summary>
    [Table(Name = "SB_CustomerServiceConfig")]
    public class CustomerServiceConfig : IEntity, ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 所属站长Id
        /// </summary>
        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        /// <summary>
        /// 分配规则
        /// </summary>
        [Column(Name = "AllocationRule")]
        public AllocationRule AllocationRule { get; set; } = AllocationRule.RoundRobin;

        /// <summary>
        /// 离线处理方式
        /// </summary>
        [Column(Name = "OfflineHandling")]
        public OfflineHandling OfflineHandling { get; set; } = OfflineHandling.LeaveMessage;

        /// <summary>
        /// 离线自动回复内容
        /// </summary>
        [Column(Name = "OfflineAutoReply", StringLength = 500)]
        public string OfflineAutoReply { get; set; }

        /// <summary>
        /// 工作时间开始（如 09:00）
        /// </summary>
        [Column(Name = "WorkTimeStart", StringLength = 10)]
        public string WorkTimeStart { get; set; }

        /// <summary>
        /// 工作时间结束（如 18:00）
        /// </summary>
        [Column(Name = "WorkTimeEnd", StringLength = 10)]
        public string WorkTimeEnd { get; set; }

        /// <summary>
        /// 是否启用工作时间限制
        /// </summary>
        [Column(Name = "EnableWorkTime")]
        public bool EnableWorkTime { get; set; } = false;

        /// <summary>
        /// 非工作时间提示语
        /// </summary>
        [Column(Name = "NonWorkTimeMessage", StringLength = 500)]
        public string NonWorkTimeMessage { get; set; }

        /// <summary>
        /// 是否启用广播接待模式（开启后新访客消息广播给所有在线客服）
        /// </summary>
        [Column(Name = "EnableBroadcast")]
        public bool EnableBroadcast { get; set; } = false;

        /// <summary>
        /// 是否启用排队功能（关闭后访客直接分配客服，无需排队）
        /// </summary>
        [Column(Name = "EnableQueue")]
        public bool EnableQueue { get; set; } = false;

        /// <summary>
        /// 最大并发会话数（启用排队时生效）
        /// </summary>
        [Column(Name = "MaxConcurrentSessions")]
        public int MaxConcurrentSessions { get; set; } = 50;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "UpdateTime", DbType = "timestamp")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 软删除标记
        /// </summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }
}
