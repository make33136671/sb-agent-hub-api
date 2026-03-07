using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 待审核域名
    /// </summary>
    [Table(Name = "SB_PendingDomain")]
    public class PendingDomain : IEntity
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 平台Id
        /// </summary>
        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        /// <summary>
        /// 所属站长Id
        /// </summary>
        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        /// <summary>
        /// 请求来源域名
        /// </summary>
        [Column(Name = "Domain", StringLength = 200, IsNullable = false)]
        public string Domain { get; set; } = "";

        /// <summary>
        /// 请求次数
        /// </summary>
        [Column(Name = "RequestCount")]
        public int RequestCount { get; set; } = 1;

        /// <summary>
        /// 最近一次请求IP
        /// </summary>
        [Column(Name = "LastIpAddress", StringLength = 50)]
        public string LastIpAddress { get; set; } = "";

        /// <summary>
        /// 最近一次 User-Agent
        /// </summary>
        [Column(Name = "LastUserAgent", StringLength = 500)]
        public string LastUserAgent { get; set; } = "";

        /// <summary>
        /// 状态: 0=待审核, 1=已批准, 2=已拒绝
        /// </summary>
        [Column(Name = "Status")]
        public PendingDomainStatus Status { get; set; } = PendingDomainStatus.Pending;

        /// <summary>
        /// 首次出现时间
        /// </summary>
        [Column(Name = "FirstSeenTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime FirstSeenTime { get; set; }

        /// <summary>
        /// 最后出现时间
        /// </summary>
        [Column(Name = "LastSeenTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime LastSeenTime { get; set; }
    }

    public enum PendingDomainStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
