using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 菜单
    /// </summary>
    [Table(Name = "sb_agenthub_menu")]
    public class Menu : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 父级Id
        /// </summary>
        [Column(Name = "ParentId")]
        public int ParentId { get; set; }

        /// <summary>父级菜单</summary>
        [Navigate(nameof(ParentId))]
        public Menu ParentMenu { get; set; } = null!;


        /// <summary>
        /// 名称
        /// </summary>
        [Required, StringLength(100)]
        [Column(Name = "Name")]
        public required string Name { get; set; }


        /// <summary>
        /// 路由路径
        /// </summary>
        [Required, StringLength(200)]
        [Column(Name = "Path")]
        public required string Path { get; set; }


        /// <summary>
        /// 组件路径
        /// </summary>
        [Column(Name = "Component")]
        public string Component { get; set; } = null!;

        /// <summary>
        /// 重定向路由
        /// </summary>
        [Column(Name = "Redirect")]
        public string Redirect { get; set; } = null!;

        /// <summary>
        /// 显示标题
        /// </summary>
        [Required, StringLength(100)]
        [Column(Name = "Title")]
        public required string Title { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Column(Name = "Icon")]
        public string Icon { get; set; } = null!;

        /// <summary>
        /// 排序编号
        /// </summary>
        [Column(Name = "OrderNo")]
        public int OrderNo { get; set; }

        /// <summary>
        /// 在菜单中隐藏
        /// </summary>
        [Column(Name = "HideInMenu")]
        public bool HideInMenu { get; set; }

        /// <summary>
        /// 忽略保持存活
        /// </summary>
        [Column(Name = "IgnoreKeepAlive")]
        public bool IgnoreKeepAlive { get; set; }

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
