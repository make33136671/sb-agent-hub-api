using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 接入平台
    /// </summary>
    [Table(Name = "SB_ChatPlatform")]
    public class ChatPlatform : IEntity, ISoftDelete
    {
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
        public string PlatformName { get; set; }

        /// <summary>
        /// 平台类型
        /// </summary>
        [Column(Name = "PlatformType")]
        public PlatformType PlatformType { get; set; } = PlatformType.Web;

        /// <summary>
        /// 平台密钥（用于 Widget 接入认证）
        /// </summary>
        [Column(Name = "PlatformKey", StringLength = 64, IsNullable = false)]
        public string PlatformKey { get; set; }

        /// <summary>
        /// 平台域名
        /// </summary>
        [Column(Name = "Domain", StringLength = 200)]
        public string Domain { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(Name = "IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 欢迎语
        /// </summary>
        [Column(Name = "WelcomeMessage", StringLength = 500)]
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }
}
