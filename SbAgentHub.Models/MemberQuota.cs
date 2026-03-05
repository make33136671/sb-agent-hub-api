using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 站长配额分配
    /// </summary>
    [Table(Name = "SB_MemberQuota")]
    public class MemberQuota : IEntity, ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 站长Id（Member.Id where Role=Owner）
        /// </summary>
        [Column(Name = "MemberId")]
        public int MemberId { get; set; }

        /// <summary>
        /// 配额标识
        /// </summary>
        [Column(Name = "QuotaKey", StringLength = 50, IsNullable = false)]
        public string QuotaKey { get; set; }

        /// <summary>
        /// 配额值
        /// </summary>
        [Column(Name = "QuotaValue")]
        public int QuotaValue { get; set; } = 0;

        /// <summary>
        /// 授权管理员Id
        /// </summary>
        [Column(Name = "GrantedBy")]
        public int? GrantedBy { get; set; }

        /// <summary>
        /// 授权者类型 Admin/System
        /// </summary>
        [Column(Name = "GrantedByType", StringLength = 10)]
        public string GrantedByType { get; set; } = "Admin";

        /// <summary>
        /// 过期时间（NULL=永不过期，四期用）
        /// </summary>
        [Column(Name = "ExpiresAt")]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "Remark", StringLength = 200)]
        public string Remark { get; set; } = "";

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
        /// 软删除
        /// </summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }
}
