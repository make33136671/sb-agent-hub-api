using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 配额实时用量（二期启用）
    /// </summary>
    [Table(Name = "SB_QuotaUsage")]
    public class QuotaUsage : IEntity, ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 站长Id
        /// </summary>
        [Column(Name = "MemberId")]
        public int MemberId { get; set; }

        /// <summary>
        /// 配额标识
        /// </summary>
        [Column(Name = "QuotaKey", StringLength = 50, IsNullable = false)]
        public string QuotaKey { get; set; }

        /// <summary>
        /// 当前使用量
        /// </summary>
        [Column(Name = "CurrentUsage")]
        public int CurrentUsage { get; set; } = 0;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "UpdateTime", DbType = "timestamp")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }
}
