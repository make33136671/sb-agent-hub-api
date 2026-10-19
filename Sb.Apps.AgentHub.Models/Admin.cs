using FreeSql.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 系统管理员
    /// </summary>
    [Table(Name = "sb_agenthub_admin")]
    [Index("ix_admin_username", "Username", true)]
    public class Admin : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 父级Id
        /// </summary>
        [Column(Name = "ParentId", IsNullable = false)]
        public int ParentId { get; set; } = 0;

        /// <summary>
        /// 用户名
        /// </summary>
        [Required, StringLength(50)]
        [Column(Name = "Username", IsNullable = false)]
        public required string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required, StringLength(200)]
        [Column(Name = "Password", IsNullable = false)]
        public required string Password { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        [Column(Name = "RoleId")]
        public int RoleId { get; set; }
        /// <summary>关联的管理员角色</summary>
        [Navigate(nameof(RoleId))]
        public AdminRole AdminRole { get; set; } = null!;

        /// <summary>
        /// 名称
        /// </summary>
        [Column(Name = "Name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// 头像
        /// </summary>
        [Column(Name = "Avatar")]
        public string Avatar { get; set; } = null!;

        /// <summary>
        /// 管理员状态
        /// </summary>
        [Column(Name = "Status", MapType = typeof(string))]
        public AdminStatus Status { get; set; } = AdminStatus.Active;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [Column(Name = "LastOnlineTime", DbType = "timestamp", IsNullable = true)]
        public DateTime? LastOnlineTime { get; set; }

        /// <summary>
        /// 伪删除
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

    /// <summary>
    /// 管理员状态
    /// </summary>
    public enum AdminStatus
    {
        /// <summary>正常</summary>
        Active = 0,
        /// <summary>禁用</summary>
        Disabled = 1,
    }
}
