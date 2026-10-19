using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 接入平台
    /// </summary>
    [Table(Name = "sb_agenthub_chat_platform")]
    [Index("ix_chatplatform_platformkey", "PlatformKey", true)]
    [Index("ix_chatplatform_ownerid", "OwnerId")]
    public class ChatPlatform : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 所属站长Id
        /// </summary>
        [Column(Name = "OwnerId")]
        public int OwnerId { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        [Column(Name = "PlatformName", StringLength = 100)]
        public string PlatformName { get; set; } = null!;

        /// <summary>
        /// 平台Logo URL
        /// </summary>
        [Column(Name = "Logo", StringLength = 500)]
        public string Logo { get; set; } = null!;

        /// <summary>
        /// 平台类型
        /// </summary>
        [Column(Name = "PlatformType", MapType = typeof(string))]
        public PlatformType PlatformType { get; set; } = PlatformType.Web;

        /// <summary>
        /// 接入方式
        /// </summary>
        [Column(Name = "AccessType", MapType = typeof(string))]
        public AccessType AccessType { get; set; } = AccessType.JS;

        /// <summary>
        /// 平台密钥（用于 Widget 接入认证）
        /// </summary>
        [Column(Name = "PlatformKey", StringLength = 64, IsNullable = false)]
        public required string PlatformKey { get; set; }

        /// <summary>
        /// 平台域名（兼容旧字段）
        /// </summary>
        [Column(Name = "Domain", StringLength = 200)]
        public string Domain { get; set; } = null!;

        /// <summary>
        /// 允许的域名列表(JSON数组，如 ["www.example.com","app.example.com"])
        /// </summary>
        [Column(Name = "AllowedDomains", DbType = "TEXT")]
        public string AllowedDomains { get; set; } = null!;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(Name = "IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 欢迎语
        /// </summary>
        [Column(Name = "WelcomeMessage", StringLength = 500)]
        public string WelcomeMessage { get; set; } = null!;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否开启管理员接管
        /// </summary>
        [Column(Name = "IsAdminTakeover", IsNullable = false, InsertValueSql = "false")]
        public bool IsAdminTakeover { get; set; }

        /// <summary>
        /// 接管管理员Id
        /// </summary>
        [Column(Name = "TakeoverAdminId", IsNullable = true)]
        public int? TakeoverAdminId { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "false")]
        public bool IsDeleted { get; set; }

        /// <summary>删除时间</summary>
        [Column(Name = "DeletedAt", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        /// <summary>删除人Id</summary>
        [Column(Name = "DeletedBy", IsNullable = true)]
        public int? DeletedBy { get; set; }
    }
}
