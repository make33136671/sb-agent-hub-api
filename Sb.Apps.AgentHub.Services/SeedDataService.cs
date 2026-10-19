using Sb.Apps.AgentHub.Models;
using FreeSql;
using Serilog;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 客服系统种子数据初始化服务
    /// 首次启动时自动初始化：默认角色 + 系统菜单 + 角色菜单关联
    /// 由 FreeSql ORM 管理 SQL，天然支持 MySQL / PostgreSQL 等多数据库
    /// </summary>
    public class SeedDataService
    {
        private readonly IFreeSql _fsql;

        public SeedDataService(IFreeSql fsql)
        {
            _fsql = fsql;
        }

        /// <summary>
        /// 执行所有种子数据初始化（幂等，可重复执行）
        /// </summary>
        public void EnsureAll()
        {
            EnsureRoles();
            EnsureMenus();
            EnsureRoleMenus();
        }

        #region 1. 默认角色

        private void EnsureRoles()
        {
            var roles = new[]
            {
                new { RoleKey = "super", Name = "超级管理员" },
                new { RoleKey = "operator", Name = "运营管理员" },
            };

            foreach (var r in roles)
            {
                var exists = _fsql.Select<AdminRole>()
                    .Where(a => a.RoleKey == r.RoleKey && !a.IsDeleted)
                    .Any();
                if (!exists)
                {
                    _fsql.Insert(new AdminRole
                    {
                        Name = r.Name,
                        RoleKey = r.RoleKey,
                        IsDeleted = false,
                    }).ExecuteAffrows();
                    Log.Information("种子数据: 新增角色 [{RoleKey}] {Name}", r.RoleKey, r.Name);
                }
            }
        }

        #endregion

        #region 2. 系统菜单

        // 菜单定义：Name, Path, Component, Title, Icon, OrderNo, ParentName(null=一级), HideInMenu, IgnoreKeepAlive
        private static readonly (string Name, string Path, string Component, string Title, string Icon, int OrderNo, string? ParentName, bool HideInMenu, bool IgnoreKeepAlive)[] _menuSeed = new[]
        {
            // ========== 一级菜单 ==========
            ("Dashboard", "/dashboard", "BasicLayout", "仪表盘", "lucide:layout-dashboard", 1, null, false, false),
            ("MemberMgmt", "/member-mgmt", "BasicLayout", "会员管理", "lucide:users", 2, null, false, false),
            ("MenuMgmt", "/menu", "BasicLayout", "菜单管理", "lucide:list-tree", 3, null, false, false),
            ("AdminMgmt", "/admin-mgmt", "BasicLayout", "管理员管理", "lucide:shield", 4, null, false, false),
            ("SystemMgmt", "/system", "BasicLayout", "系统设置", "lucide:settings", 5, null, false, false),
            ("QuotaMgmt", "/quota-mgmt", "BasicLayout", "配额管理", "lucide:gauge", 6, null, false, false),
            ("OrderMgmt", "/order-mgmt", "BasicLayout", "订单管理", "lucide:receipt", 7, null, false, false),
            ("Workbench", "/workbench", "BasicLayout", "客服工作台", "lucide:headphones", 10, null, false, false),
            ("PlatformMgmt", "/platform", "BasicLayout", "平台管理", "lucide:globe", 11, null, false, false),
            ("AgentMgmt", "/agent-mgmt", "BasicLayout", "客服管理", "lucide:user-check", 12, null, false, false),
            ("UserMgmt", "/user-mgmt", "BasicLayout", "用户管理", "lucide:message-circle", 13, null, false, false),
            ("CsConfig", "/cs-config", "BasicLayout", "客服配置", "lucide:sliders", 14, null, false, false),
            ("VisitorFormMgmt", "/visitor-form-mgmt", "BasicLayout", "访客收集", "lucide:clipboard-list", 30, null, false, false),
            ("AutoReplyMgmt", "/auto-reply-mgmt", "BasicLayout", "自动回复", "lucide:message-square-reply", 31, null, false, false),
            ("SatisfactionMgmt", "/satisfaction-mgmt", "BasicLayout", "满意度评价", "lucide:star", 32, null, false, false),
            ("DataExportMgmt", "/data-export-mgmt", "BasicLayout", "数据导出", "lucide:download", 33, null, false, false),
            ("ConversationMgmt", "/conversation-mgmt", "BasicLayout", "会话管理", "lucide:messages-square", 81, null, false, false),
            ("Profile", "/profile", "views/_core/profile/index.vue", "个人中心", "lucide:user", 9999, null, true, false),

            // ========== 二级菜单 ==========
            // Dashboard
            ("DashboardIndex", "/dashboard/index", "/dashboard/workspace/index", "系统概览", "", 1, "Dashboard", false, false),
            // 会员管理
            ("OwnerList", "/member-mgmt/owner", "/member/List", "站长管理", "", 1, "MemberMgmt", false, false),
            ("AgentList", "/member-mgmt/agent", "/member/List", "客服管理", "", 2, "MemberMgmt", false, false),
            ("MemberForm", "/member-mgmt/edit", "/member/Form", "编辑成员", "", 3, "MemberMgmt", true, false),
            // 菜单管理
            ("MenuList", "/menu/list", "/menu/List", "菜单列表", "", 1, "MenuMgmt", false, false),
            ("MenuForm", "/menu/edit", "/menu/Form", "编辑菜单", "", 2, "MenuMgmt", true, false),
            ("MenuTree", "/menu/tree", "/menu/Tree", "菜单树", "", 3, "MenuMgmt", false, false),
            // 管理员管理
            ("AdminList", "/admin-mgmt/list", "/admin/List", "管理员列表", "", 1, "AdminMgmt", false, false),
            ("AdminEdit", "/admin-mgmt/edit", "/admin/Form", "编辑管理员", "", 2, "AdminMgmt", true, false),
            ("RoleList", "/admin-mgmt/role", "/admin/RoleList", "角色管理", "", 3, "AdminMgmt", false, false),
            ("RoleEdit", "/admin-mgmt/role-edit", "/admin/RoleForm", "编辑角色", "", 4, "AdminMgmt", true, false),
            // 系统设置
            ("SystemSetting", "/system/setting", "/system-setting/List", "系统参数", "", 1, "SystemMgmt", false, false),
            ("LoginLog", "/system/login-log", "/login-log/List", "登录日志", "ion:log-in-outline", 7, "SystemMgmt", false, false),
            // 配额管理
            ("QuotaList", "/quota-mgmt/list", "/quota/List", "配额分配", "", 1, "QuotaMgmt", false, false),
            ("QuotaDashboard", "/quota-mgmt/dashboard", "/quota/Dashboard", "用量概览", "", 2, "QuotaMgmt", false, false),
            ("QuotaPackageList", "/quota-mgmt/packages", "/quota-package/List", "套餐模板", "", 3, "QuotaMgmt", false, false),
            // 订单管理
            ("OrderList", "/order-mgmt/list", "/order/List", "订单列表", "", 1, "OrderMgmt", false, false),
            // 客服工作台
            ("WorkbenchIndex", "/workbench/index", "/workbench/Index", "工作台", "", 1, "Workbench", false, false),
            // 平台管理
            ("PlatformList", "/platform/list", "/platform/List", "接入平台", "", 1, "PlatformMgmt", false, false),
            ("PlatformForm", "/platform/edit", "/platform/Form", "编辑平台", "", 2, "PlatformMgmt", true, false),
            ("AccessViolationList", "/platform/violations", "/access-violation/List", "接入日志", "", 3, "PlatformMgmt", false, false),
            // 客服管理
            ("MyAgentList", "/agent-mgmt/list", "/agent/List", "我的客服", "", 1, "AgentMgmt", false, false),
            ("AgentForm", "/agent-mgmt/edit", "/agent/Form", "编辑客服", "", 2, "AgentMgmt", true, false),
            // 用户管理
            ("ChatUserList", "/user-mgmt/list", "/chat-user/List", "聊天用户", "", 1, "UserMgmt", false, false),
            // 客服配置
            ("CsConfigIndex", "/cs-config/index", "/cs-config/Index", "分配规则", "", 1, "CsConfig", false, false),
            // 访客收集
            ("VisitorFormList", "/visitor-form-mgmt/list", "/visitor-form/List", "表单管理", "", 1, "VisitorFormMgmt", false, false),
            // 自动回复
            ("AutoReplyList", "/auto-reply-mgmt/list", "/auto-reply/List", "规则管理", "", 1, "AutoReplyMgmt", false, false),
            // 满意度评价
            ("SatisfactionList", "/satisfaction-mgmt/list", "/satisfaction/List", "评价列表", "", 1, "SatisfactionMgmt", false, false),
            // 数据导出
            ("DataExportList", "/data-export-mgmt/list", "/data-export/List", "导出管理", "", 1, "DataExportMgmt", false, false),
            // 会话管理
            ("ConversationList", "/conversation-mgmt/list", "/conversation/List", "会话列表", "", 1, "ConversationMgmt", false, false),

            // ========== 系统管理 (sb框架功能菜单，使用 /sys-admin 避免与客服系统 /system 冲突) ==========
            ("System", "/sys-admin", "BasicLayout", "系统管理", "ion:settings-outline", 90, null, false, false),
            // 数据库与缓存
            ("DbCacheMgmt", "/sys-admin/db-cache", "", "数据库与缓存", "ion:server-outline", 10, "System", false, false),
            ("Database", "/sys-admin/database", "/database/List", "数据库管理", "ion:server-outline", 1, "DbCacheMgmt", false, false),
            ("Cache", "/sys-admin/cache", "/cache/List", "缓存管理", "ion:server-outline", 2, "DbCacheMgmt", false, false),
            // 邮件管理
            ("EmailMgmt", "/sys-admin/email", "", "邮件管理", "ion:mail-outline", 11, "System", false, false),
            ("EmailConfig", "/sys-admin/email/config", "/email/config/index", "邮件配置", "ion:mail-outline", 1, "EmailMgmt", false, false),
            ("EmailLog", "/sys-admin/email/log", "/email/log/index", "邮件日志", "ion:mail-unread-outline", 2, "EmailMgmt", false, false),
            ("EmailTemplate", "/sys-admin/email/template", "/email/template/index", "邮件模板", "ion:mail-open-outline", 3, "EmailMgmt", false, false),
            // 授权管理
            // 系统工具
            ("SysToolMgmt", "/sys-admin/sys-tool", "", "系统工具", "ion:build-outline", 13, "System", false, false),
            ("SqlTool", "/sys-admin/sql", "/sql/List", "SQL工具", "ion:code-slash-outline", 1, "SysToolMgmt", false, false),
            ("AppSettings", "/sys-admin/app-settings", "/app-settings/index", "应用设置", "ion:apps-outline", 2, "SysToolMgmt", false, false),
            ("AccessToken", "/sys-admin/access-token", "/access-token/index", "API令牌", "ion:key-outline", 3, "SysToolMgmt", false, false),
            ("RealName", "/sys-admin/real-name", "/real-name/List", "实名认证", "ion:card-outline", 4, "SysToolMgmt", false, false),
            // 运维与部署
            ("OpsDeployMgmt", "/sys-admin/ops-deploy", "", "运维与部署", "ion:rocket-outline", 14, "System", false, false),
            ("BackgroundTask", "/sys-admin/background-task", "/background-task/List", "后台任务", "ion:timer-outline", 1, "OpsDeployMgmt", false, false),
            ("LogViewer", "/sys-admin/log-viewer", "/log-viewer/List", "日志查看", "ion:document-text-outline", 2, "OpsDeployMgmt", false, false),
            ("BackupReceive", "/sys-admin/backup-receive", "/backup-receive/ProductList", "备份接收", "ion:cloud-download-outline", 3, "OpsDeployMgmt", false, false),
            ("Deploy", "/sys-admin/deploy", "/deploy/List", "部署管理", "ion:cloud-upload-outline", 4, "OpsDeployMgmt", false, false),
            ("OpsTools", "/sys-admin/ops-tools", "/ops-tools/index", "运维工具", "ion:construct-outline", 5, "OpsDeployMgmt", false, false),
        };

        private void EnsureMenus()
        {
            int added = 0;
            int updated = 0;

            foreach (var m in _menuSeed)
            {
                var existing = _fsql.Select<Menu>()
                    .Where(x => x.Name == m.Name && !x.IsDeleted)
                    .First();

                int parentId = 0;
                if (m.ParentName != null)
                {
                    var parent = _fsql.Select<Menu>()
                        .Where(x => x.Name == m.ParentName && !x.IsDeleted)
                        .First();
                    if (parent != null)
                        parentId = parent.Id;
                }

                if (existing != null)
                {
                    // 已存在则更新（修正之前可能错误的种子数据）
                    bool needUpdate =
                        existing.ParentId != parentId ||
                        existing.Path != m.Path ||
                        existing.Component != m.Component ||
                        existing.Title != m.Title ||
                        existing.Icon != m.Icon ||
                        existing.OrderNo != m.OrderNo ||
                        existing.HideInMenu != m.HideInMenu ||
                        existing.IgnoreKeepAlive != m.IgnoreKeepAlive;

                    if (needUpdate)
                    {
                        _fsql.Update<Menu>()
                            .Set(x => x.ParentId == parentId)
                            .Set(x => x.Path == m.Path)
                            .Set(x => x.Component == m.Component)
                            .Set(x => x.Title == m.Title)
                            .Set(x => x.Icon == m.Icon)
                            .Set(x => x.OrderNo == m.OrderNo)
                            .Set(x => x.HideInMenu == m.HideInMenu)
                            .Set(x => x.IgnoreKeepAlive == m.IgnoreKeepAlive)
                            .Where(x => x.Id == existing.Id)
                            .ExecuteAffrows();
                        updated++;
                    }
                    continue;
                }

                _fsql.Insert(new Menu
                {
                    ParentId = parentId,
                    Name = m.Name,
                    Path = m.Path,
                    Component = m.Component,
                    Redirect = "",
                    Title = m.Title,
                    Icon = m.Icon,
                    OrderNo = m.OrderNo,
                    HideInMenu = m.HideInMenu,
                    IgnoreKeepAlive = m.IgnoreKeepAlive,
                    IsDeleted = false,
                }).ExecuteAffrows();
                added++;
            }

            if (added > 0)
                Log.Information("种子数据: 新增 {Count} 个菜单", added);
            if (updated > 0)
                Log.Information("种子数据: 更新 {Count} 个菜单", updated);
        }

        #endregion

        #region 3. 角色菜单关联

        private static readonly string[] _operatorMenus = new[]
        {
            "Dashboard", "DashboardIndex",
            "Workbench", "WorkbenchIndex",
            "MemberMgmt", "OwnerList", "AgentList", "MemberForm",
            "PlatformMgmt", "PlatformList", "PlatformForm", "AccessViolationList",
            "AgentMgmt", "MyAgentList", "AgentForm",
            "UserMgmt", "ChatUserList",
            "CsConfig", "CsConfigIndex",
            "ConversationMgmt", "ConversationList",
            "VisitorFormMgmt", "VisitorFormList",
            "AutoReplyMgmt", "AutoReplyList",
            "SatisfactionMgmt", "SatisfactionList",
            "DataExportMgmt", "DataExportList",
            "OrderMgmt", "OrderList",
            "Profile",
        };

        private void EnsureRoleMenus()
        {
            var superRole = _fsql.Select<AdminRole>()
                .Where(r => r.RoleKey == "super" && !r.IsDeleted)
                .First();
            var opRole = _fsql.Select<AdminRole>()
                .Where(r => r.RoleKey == "operator" && !r.IsDeleted)
                .First();

            if (superRole == null && opRole == null) return;

            // super 角色：分配所有菜单
            if (superRole != null)
            {
                var allMenuIds = _fsql.Select<Menu>()
                    .Where(m => !m.IsDeleted)
                    .ToList(m => m.Id);

                var existingMenuIds = _fsql.Select<AdminRoleMenu>()
                    .Where(rm => rm.RoleId == superRole.Id)
                    .ToList(rm => rm.MenuId);

                var toAdd = allMenuIds.Except(existingMenuIds).ToList();
                if (toAdd.Count > 0)
                {
                    _fsql.Insert(toAdd.Select(id => new AdminRoleMenu
                    {
                        RoleId = superRole.Id,
                        MenuId = id,
                    }).ToList()).ExecuteAffrows();
                    Log.Information("种子数据: 超级管理员新增 {Count} 个菜单权限", toAdd.Count);
                }
            }

            // operator 角色：分配运营相关菜单
            if (opRole != null)
            {
                // 先查所有菜单，内存过滤（避免 FreeSql PostgreSQL 提供程序 Contains 翻译问题）
                var allMenus = _fsql.Select<Menu>()
                    .Where(m => !m.IsDeleted)
                    .ToList(m => new { m.Id, m.Name });

                var opMenuIds = allMenus
                    .Where(m => _operatorMenus.Contains(m.Name))
                    .Select(m => m.Id)
                    .ToList();

                var existingMenuIds = _fsql.Select<AdminRoleMenu>()
                    .Where(rm => rm.RoleId == opRole.Id)
                    .ToList(rm => rm.MenuId);

                var toAdd = opMenuIds.Except(existingMenuIds).ToList();
                if (toAdd.Count > 0)
                {
                    _fsql.Insert(toAdd.Select(id => new AdminRoleMenu
                    {
                        RoleId = opRole.Id,
                        MenuId = id,
                    }).ToList()).ExecuteAffrows();
                    Log.Information("种子数据: 运营管理员新增 {Count} 个菜单权限", toAdd.Count);
                }
            }
        }

        #endregion
    }
}
