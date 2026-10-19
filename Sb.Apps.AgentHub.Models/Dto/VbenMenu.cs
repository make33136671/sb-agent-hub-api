using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sb.Apps.AgentHub.Models.Dto
{
    /// <summary>
    /// Vben 前端菜单模型
    /// </summary>
    public class VbenMenu
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="menu">菜单实体</param>
        /// <param name="allMenus">全部菜单列表（传入后自动递归构建子菜单）</param>
        public VbenMenu(Menu menu, List<Menu>? allMenus = null)
        {
            Id = menu.Id;
            ParentId = menu.ParentId;
            Name = menu.Name;
            Path = menu.Path;
            Component = string.IsNullOrEmpty(menu.Component) ? "PARENT" : menu.Component;
            Redirect = menu.Redirect!;
            Meta = new Meta
            {
                Title = menu.Title,
                Icon = menu.Icon,
                OrderNo = menu.OrderNo,
                HideInMenu = menu.HideInMenu,
                IgnoreKeepAlive = menu.IgnoreKeepAlive,
            };

            if (allMenus != null)
            {
                Children = allMenus
                    .Where(m => m.ParentId == menu.Id)
                    .OrderBy(m => m.OrderNo)
                    .Select(m => new VbenMenu(m, allMenus))
                    .ToList();
            }
            else
            {
                Children = new List<VbenMenu>();
            }
        }

        /// <summary>
        /// 菜单ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 父级ID
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// 路由路径
        /// </summary>
        public string Path { get; set; } = null!;
        /// <summary>
        /// 路由名称
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// 组件路径
        /// </summary>
        public string Component { get; set; } = null!;
        /// <summary>
        /// 重定向路径
        /// </summary>
        public string Redirect { get; set; } = null!;
        /// <summary>
        /// 菜单元数据
        /// </summary>
        public Meta Meta { get; set; } = null!;
        /// <summary>
        /// 子菜单列表
        /// </summary>
        public List<VbenMenu> Children { get; set; } = null!;

        /// <summary>
        /// 便利属性 — 返回 Meta.Title
        /// </summary>
        public string Title => Meta?.Title!;

        /// <summary>
        /// 便利属性 — 返回 Meta.OrderNo
        /// </summary>
        public int OrderNo => Meta?.OrderNo ?? 0;
    }

    /// <summary>
    /// 菜单元数据
    /// </summary>
    public class Meta
    {
        /// <summary>
        /// 菜单标题
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// 菜单图标
        /// </summary>
        public string Icon { get; set; } = null!;
        /// <summary>
        /// 排序编号
        /// </summary>
        public int OrderNo { get; set; }
        /// <summary>
        /// 在菜单中隐藏
        /// </summary>
        public bool HideInMenu { get; set; }
        /// <summary>
        /// 忽略缓存
        /// </summary>
        public bool IgnoreKeepAlive { get; set; }
    }
}
