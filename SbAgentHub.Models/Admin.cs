using FreeSql.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 系统管理员
    /// </summary>
    [Table(Name = "SB_Admin")]
    public class Admin : IEntity, ISoftDelete
    {
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
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required, StringLength(200)]
        [Column(Name = "Password", IsNullable = false)]
        public string Password { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        [Column(Name = "RoleId")]
        public int RoleId { get; set; }
        [Navigate(nameof(RoleId))]
        public AdminRole AdminRole { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [Column(Name = "Avatar")]
        public string Avatar { get; set; }

        /// <summary>
        /// 管理员状态
        /// </summary>
        [Column(Name = "Status")]
        public AdminStatus Status { get; set; } = AdminStatus.Active;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreatedTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [Column(Name = "LastOnlineTime", DbType = "timestamp", IsNullable = true)]
        public DateTime? LastOnlineTime { get; set; }

        /// <summary>
        /// 伪删除
        /// </summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }

    public enum AdminStatus
    {
        Active = 0,
        Disabled = 1,
    }
}
