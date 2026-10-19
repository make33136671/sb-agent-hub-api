using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 管理员角色
    /// </summary>
    [Table(Name = "sb_agenthub_admin_role")]
    public class AdminRole : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }


        /// <summary>
        /// 角色名称
        /// </summary>
        [Required, StringLength(50)]
        [Column(Name = "Name")]
        public required string Name { get; set; }

        /// <summary>
        /// 角色标识
        /// </summary>
        [Required, StringLength(50)]
        [Column(Name = "RoleKey")]
        public required string RoleKey { get; set; }


        /// <summary>关联的管理员列表</summary>
        [Navigate(nameof(Admin.RoleId))]
        public List<Admin> Admins { get; set; } = null!;

        /// <summary>
        /// 角色菜单关联
        /// </summary>
        [Navigate(nameof(AdminRoleMenu.RoleId))]
        public List<AdminRoleMenu> RoleMenu { get; set; } = null!;

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
}
