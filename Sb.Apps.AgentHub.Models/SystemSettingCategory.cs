using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 系统设置分类
    /// </summary>
    [Table(Name = "sb_agenthub_system_setting_category")]
    public class SystemSettingCategory : IEntity
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 设置名称
        /// </summary>
        [Required, StringLength(50)]
        [Column(Name = "Name")]
        public required string Name { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Column(Name = "Icon")]
        public string Icon { get; set; } = null!;

        /// <summary>
        /// 显示排序
        /// </summary>
        [Column(Name = "Sort")]
        public int Sort { get; set; }

        /// <summary>关联的系统设置列表</summary>
        [Navigate(nameof(SystemSetting.CategoryId))]
        public List<SystemSetting> SystemSettingList { get; set; } = null!;

    }

}
