-- ========================================
-- 赛博客服系统 - 全量菜单种子数据（幂等，可重复执行）
-- 覆盖：seed.sql + migration_menus / quota / phase3 / phase4 / profile / errorlog 全部菜单
-- 用法：mysql -u root -p 数据库名 < full_menu_seed.sql
-- 作用：sb_agenthub_menu 为空 / 登录后菜单为空时，执行本脚本一次性修复
-- ========================================

-- ========================================
-- 0. 确保默认角色存在
-- ========================================
INSERT INTO sb_agenthub_admin_role (Name, RoleKey, IsDeleted)
SELECT '超级管理员', 'super', 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_admin_role WHERE RoleKey = 'super');

INSERT INTO sb_agenthub_admin_role (Name, RoleKey, IsDeleted)
SELECT '运营管理员', 'operator', 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_admin_role WHERE RoleKey = 'operator');

-- ========================================
-- 1. 一级菜单（父级）
-- ========================================
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'Dashboard', '/dashboard', 'BasicLayout', NULL, '仪表盘', 'lucide:layout-dashboard', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'Dashboard' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'MemberMgmt', '/member-mgmt', 'BasicLayout', NULL, '会员管理', 'lucide:users', 2, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'MemberMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'MenuMgmt', '/menu', 'BasicLayout', NULL, '菜单管理', 'lucide:list-tree', 3, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'MenuMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'AdminMgmt', '/admin-mgmt', 'BasicLayout', NULL, '管理员管理', 'lucide:shield', 4, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AdminMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'SystemMgmt', '/system', 'BasicLayout', NULL, '系统设置', 'lucide:settings', 5, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'SystemMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'QuotaMgmt', '/quota-mgmt', 'BasicLayout', NULL, '配额管理', 'lucide:gauge', 6, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'QuotaMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'OrderMgmt', '/order-mgmt', 'BasicLayout', NULL, '订单管理', 'lucide:receipt', 7, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'OrderMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'Workbench', '/workbench', 'BasicLayout', NULL, '客服工作台', 'lucide:headphones', 10, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'Workbench' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'PlatformMgmt', '/platform', 'BasicLayout', NULL, '平台管理', 'lucide:globe', 11, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'PlatformMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'AgentMgmt', '/agent-mgmt', 'BasicLayout', NULL, '客服管理', 'lucide:user-check', 12, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AgentMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'UserMgmt', '/user-mgmt', 'BasicLayout', NULL, '用户管理', 'lucide:message-circle', 13, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'UserMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'CsConfig', '/cs-config', 'BasicLayout', NULL, '客服配置', 'lucide:sliders', 14, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'CsConfig' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'VisitorFormMgmt', '/visitor-form-mgmt', 'BasicLayout', NULL, '访客收集', 'lucide:clipboard-list', 30, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'VisitorFormMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'AutoReplyMgmt', '/auto-reply-mgmt', 'BasicLayout', NULL, '自动回复', 'lucide:message-square-reply', 31, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AutoReplyMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'SatisfactionMgmt', '/satisfaction-mgmt', 'BasicLayout', NULL, '满意度评价', 'lucide:star', 32, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'SatisfactionMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'DataExportMgmt', '/data-export-mgmt', 'BasicLayout', NULL, '数据导出', 'lucide:download', 33, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'DataExportMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'ConversationMgmt', '/conversation-mgmt', 'BasicLayout', NULL, '会话管理', 'lucide:messages-square', 81, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'ConversationMgmt' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT 0, 'Profile', '/profile', 'views/_core/profile/index.vue', NULL, '个人中心', 'lucide:user', 9999, 1, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'Profile' AND IsDeleted = 0);

-- ========================================
-- 2. 二级菜单（子级）
-- ========================================
-- Dashboard
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'Dashboard' AND IsDeleted = 0 LIMIT 1), 'DashboardIndex', '/dashboard/index', '/dashboard/workspace/index', NULL, '系统概览', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'DashboardIndex' AND IsDeleted = 0);

-- 会员管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'MemberMgmt' AND IsDeleted = 0 LIMIT 1), 'OwnerList', '/member-mgmt/owner', '/member/List', NULL, '站长管理', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'OwnerList' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'MemberMgmt' AND IsDeleted = 0 LIMIT 1), 'AgentList', '/member-mgmt/agent', '/member/List', NULL, '客服管理', '', 2, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AgentList' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'MemberMgmt' AND IsDeleted = 0 LIMIT 1), 'MemberForm', '/member-mgmt/edit', '/member/Form', NULL, '编辑成员', '', 3, 1, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'MemberForm' AND IsDeleted = 0);

-- 菜单管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'MenuMgmt' AND IsDeleted = 0 LIMIT 1), 'MenuList', '/menu/list', '/menu/List', NULL, '菜单列表', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'MenuList' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'MenuMgmt' AND IsDeleted = 0 LIMIT 1), 'MenuForm', '/menu/edit', '/menu/Form', NULL, '编辑菜单', '', 2, 1, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'MenuForm' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'MenuMgmt' AND IsDeleted = 0 LIMIT 1), 'MenuTree', '/menu/tree', '/menu/Tree', NULL, '菜单树', '', 3, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'MenuTree' AND IsDeleted = 0);

-- 管理员管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'AdminMgmt' AND IsDeleted = 0 LIMIT 1), 'AdminList', '/admin-mgmt/list', '/admin/List', NULL, '管理员列表', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AdminList' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'AdminMgmt' AND IsDeleted = 0 LIMIT 1), 'AdminEdit', '/admin-mgmt/edit', '/admin/Form', NULL, '编辑管理员', '', 2, 1, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AdminEdit' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'AdminMgmt' AND IsDeleted = 0 LIMIT 1), 'RoleList', '/admin-mgmt/role', '/admin/RoleList', NULL, '角色管理', '', 3, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'RoleList' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'AdminMgmt' AND IsDeleted = 0 LIMIT 1), 'RoleEdit', '/admin-mgmt/role-edit', '/admin/RoleForm', NULL, '编辑角色', '', 4, 1, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'RoleEdit' AND IsDeleted = 0);

-- 系统设置
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'SystemMgmt' AND IsDeleted = 0 LIMIT 1), 'SystemSetting', '/system/setting', '/system-setting/List', NULL, '系统参数', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'SystemSetting' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'SystemMgmt' AND IsDeleted = 0 LIMIT 1), 'LoginLog', '/system/login-log', '/login-log/List', NULL, '登录日志', 'ion:log-in-outline', 7, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'LoginLog' AND IsDeleted = 0);

-- 配额管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'QuotaMgmt' AND IsDeleted = 0 LIMIT 1), 'QuotaList', '/quota-mgmt/list', '/quota/List', NULL, '配额分配', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'QuotaList' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'QuotaMgmt' AND IsDeleted = 0 LIMIT 1), 'QuotaDashboard', '/quota-mgmt/dashboard', '/quota/Dashboard', NULL, '用量概览', '', 2, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'QuotaDashboard' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'QuotaMgmt' AND IsDeleted = 0 LIMIT 1), 'QuotaPackageList', '/quota-mgmt/packages', '/quota-package/List', NULL, '套餐模板', '', 3, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'QuotaPackageList' AND IsDeleted = 0);

-- 订单管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'OrderMgmt' AND IsDeleted = 0 LIMIT 1), 'OrderList', '/order-mgmt/list', '/order/List', NULL, '订单列表', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'OrderList' AND IsDeleted = 0);

-- 客服工作台
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'Workbench' AND IsDeleted = 0 LIMIT 1), 'WorkbenchIndex', '/workbench/index', '/workbench/Index', NULL, '工作台', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'WorkbenchIndex' AND IsDeleted = 0);

-- 平台管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'PlatformMgmt' AND IsDeleted = 0 LIMIT 1), 'PlatformList', '/platform/list', '/platform/List', NULL, '接入平台', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'PlatformList' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'PlatformMgmt' AND IsDeleted = 0 LIMIT 1), 'PlatformForm', '/platform/edit', '/platform/Form', NULL, '编辑平台', '', 2, 1, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'PlatformForm' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'PlatformMgmt' AND IsDeleted = 0 LIMIT 1), 'AccessViolationList', '/platform/violations', '/access-violation/List', NULL, '接入日志', '', 3, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AccessViolationList' AND IsDeleted = 0);

-- 客服管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'AgentMgmt' AND IsDeleted = 0 LIMIT 1), 'MyAgentList', '/agent-mgmt/list', '/agent/List', NULL, '我的客服', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'MyAgentList' AND IsDeleted = 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'AgentMgmt' AND IsDeleted = 0 LIMIT 1), 'AgentForm', '/agent-mgmt/edit', '/agent/Form', NULL, '编辑客服', '', 2, 1, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AgentForm' AND IsDeleted = 0);

-- 用户管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'UserMgmt' AND IsDeleted = 0 LIMIT 1), 'ChatUserList', '/user-mgmt/list', '/chat-user/List', NULL, '聊天用户', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'ChatUserList' AND IsDeleted = 0);

-- 客服配置
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'CsConfig' AND IsDeleted = 0 LIMIT 1), 'CsConfigIndex', '/cs-config/index', '/cs-config/Index', NULL, '分配规则', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'CsConfigIndex' AND IsDeleted = 0);

-- 访客收集
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'VisitorFormMgmt' AND IsDeleted = 0 LIMIT 1), 'VisitorFormList', '/visitor-form-mgmt/list', '/visitor-form/List', NULL, '表单管理', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'VisitorFormList' AND IsDeleted = 0);

-- 自动回复
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'AutoReplyMgmt' AND IsDeleted = 0 LIMIT 1), 'AutoReplyList', '/auto-reply-mgmt/list', '/auto-reply/List', NULL, '规则管理', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'AutoReplyList' AND IsDeleted = 0);

-- 满意度评价
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'SatisfactionMgmt' AND IsDeleted = 0 LIMIT 1), 'SatisfactionList', '/satisfaction-mgmt/list', '/satisfaction/List', NULL, '评价列表', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'SatisfactionList' AND IsDeleted = 0);

-- 数据导出
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'DataExportMgmt' AND IsDeleted = 0 LIMIT 1), 'DataExportList', '/data-export-mgmt/list', '/data-export/List', NULL, '导出管理', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'DataExportList' AND IsDeleted = 0);

-- 会话管理
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
SELECT (SELECT Id FROM sb_agenthub_menu WHERE Name = 'ConversationMgmt' AND IsDeleted = 0 LIMIT 1), 'ConversationList', '/conversation-mgmt/list', '/conversation/List', NULL, '会话列表', '', 1, 0, 0, 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE Name = 'ConversationList' AND IsDeleted = 0);

-- ========================================
-- 3. 菜单 -> 角色关联（super 分配全部菜单；operator 分配运营相关菜单）
-- ========================================
INSERT INTO sb_agenthub_admin_role_menu (RoleId, MenuId)
SELECT r.Id, m.Id
FROM sb_agenthub_admin_role r, sb_agenthub_menu m
WHERE r.RoleKey = 'super'
  AND m.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM sb_agenthub_admin_role_menu rm WHERE rm.RoleId = r.Id AND rm.MenuId = m.Id);

INSERT INTO sb_agenthub_admin_role_menu (RoleId, MenuId)
SELECT r.Id, m.Id
FROM sb_agenthub_admin_role r, sb_agenthub_menu m
WHERE r.RoleKey = 'operator'
  AND m.IsDeleted = 0
  AND m.Name IN (
    'Dashboard', 'DashboardIndex',
    'Workbench', 'WorkbenchIndex',
    'MemberMgmt', 'OwnerList', 'AgentList', 'MemberForm',
    'PlatformMgmt', 'PlatformList', 'PlatformForm', 'AccessViolationList',
    'AgentMgmt', 'MyAgentList', 'AgentForm',
    'UserMgmt', 'ChatUserList',
    'CsConfig', 'CsConfigIndex',
    'ConversationMgmt', 'ConversationList',
    'VisitorFormMgmt', 'VisitorFormList',
    'AutoReplyMgmt', 'AutoReplyList',
    'SatisfactionMgmt', 'SatisfactionList',
    'DataExportMgmt', 'DataExportList',
    'OrderMgmt', 'OrderList',
    'Profile'
  )
  AND NOT EXISTS (SELECT 1 FROM sb_agenthub_admin_role_menu rm WHERE rm.RoleId = r.Id AND rm.MenuId = m.Id);

-- ========================================
-- 4. 校验
-- ========================================
-- SELECT COUNT(*) FROM sb_agenthub_menu WHERE IsDeleted = 0;
-- SELECT COUNT(*) FROM sb_agenthub_admin_role_menu;
