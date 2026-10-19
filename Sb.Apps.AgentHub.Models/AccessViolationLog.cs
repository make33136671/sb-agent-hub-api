using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 非法接入记录
    /// </summary>
    [Table(Name = "sb_agenthub_access_violation_log")]
    [Index("ix_accessviolation_platformid", "PlatformId")]
    [Index("ix_accessviolation_createtime", "CreateTime")]
    public class AccessViolationLog : IEntity
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 平台Id
        /// </summary>
        [Column(Name = "PlatformId")]
        public int PlatformId { get; set; }

        /// <summary>
        /// 使用的平台Key
        /// </summary>
        [Column(Name = "PlatformKey", StringLength = 64, IsNullable = false)]
        public required string PlatformKey { get; set; }

        /// <summary>
        /// 所属站长Id
        /// </summary>
        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        /// <summary>
        /// 请求来源域名
        /// </summary>
        [Column(Name = "RequestDomain", StringLength = 200)]
        public string RequestDomain { get; set; } = "";

        /// <summary>
        /// 请求来源类型: js/vue/chat
        /// </summary>
        [Column(Name = "RequestSource", StringLength = 20, IsNullable = false)]
        public required string RequestSource { get; set; }

        /// <summary>
        /// 期望的接入类型
        /// </summary>
        [Column(Name = "ExpectedSource", StringLength = 20, IsNullable = false)]
        public required string ExpectedSource { get; set; }

        /// <summary>
        /// 拒绝原因: domain_mismatch / type_mismatch / platform_disabled / quota_exceeded
        /// </summary>
        [Column(Name = "Reason", StringLength = 50, IsNullable = false)]
        public required string Reason { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [Column(Name = "IpAddress", StringLength = 50)]
        public string IpAddress { get; set; } = "";

        /// <summary>
        /// User-Agent
        /// </summary>
        [Column(Name = "UserAgent", StringLength = 500)]
        public string UserAgent { get; set; } = "";

        /// <summary>
        /// 请求的完整URL
        /// </summary>
        [Column(Name = "RequestUrl", StringLength = 500)]
        public string RequestUrl { get; set; } = "";

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }
    }
}
