-- ========================================
-- 赛博客服系统 种子数据
-- ========================================

-- 初始管理员（密码: 111111）
INSERT INTO sb_agenthub_admin (ParentId, Username, Password, RoleId, Name, Status, CreatedTime, IsDeleted) VALUES
(0, 'admin', '$2a$11$p7KGZYILT910BSkkevr2q.XqMsvMqFMPCFO1alVPq8TusYZv3KqlW', 1, '超级管理员', 0, NOW(), 0);

-- 默认角色
INSERT INTO sb_agenthub_admin_role (Name, RoleKey, IsDeleted) VALUES
('超级管理员', 'super', 0),
('运营管理员', 'operator', 0);

-- ========================================
-- 菜单种子数据
-- Name = 路由名称(英文), Title = 显示标题(中文)
-- ========================================

-- 一级菜单（Layout 容器）
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IsDeleted) VALUES
-- Admin 菜单 (OrderNo 1-9)
(0, 'Dashboard',    '/dashboard',   'BasicLayout', '仪表盘',     'lucide:layout-dashboard', 1, 0, 0),
(0, 'MemberMgmt',   '/member-mgmt', 'BasicLayout', '会员管理',   'lucide:users',            2, 0, 0),
(0, 'MenuMgmt',     '/menu',        'BasicLayout', '菜单管理',   'lucide:list-tree',        3, 0, 0),
(0, 'AdminMgmt',    '/admin-mgmt',  'BasicLayout', '管理员管理', 'lucide:shield',           4, 0, 0),
(0, 'SystemMgmt',   '/system',      'BasicLayout', '系统设置',   'lucide:settings',         5, 0, 0),

-- Owner 菜单 (OrderNo 10+)
(0, 'Workbench',    '/workbench',   'BasicLayout', '客服工作台', 'lucide:headphones',       10, 0, 0),
(0, 'PlatformMgmt', '/platform',    'BasicLayout', '平台管理',   'lucide:globe',            11, 0, 0),
(0, 'AgentMgmt',    '/agent-mgmt',  'BasicLayout', '客服管理',   'lucide:user-check',       12, 0, 0),
(0, 'UserMgmt',     '/user-mgmt',   'BasicLayout', '用户管理',   'lucide:message-circle',   13, 0, 0),
(0, 'CsConfig',     '/cs-config',   'BasicLayout', '客服配置',   'lucide:sliders',          14, 0, 0);

-- 二级菜单（Admin 页面）— ParentId 对应上面一级菜单的自增 Id
-- 假设一级菜单 Id 为 1-10
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IsDeleted) VALUES
-- Dashboard 子菜单
(1, 'DashboardIndex', '/dashboard/index', '/dashboard/workspace/index', '系统概览', '', 1, 0, 0),

-- 会员管理子菜单
(2, 'OwnerList', '/member-mgmt/owner', '/member/List', '站长管理', '', 1, 0, 0),
(2, 'AgentList', '/member-mgmt/agent', '/member/List', '客服管理', '', 2, 0, 0),
(2, 'MemberForm', '/member-mgmt/edit', '/member/Form', '编辑成员', '', 3, 1, 0),

-- 菜单管理子菜单
(3, 'MenuList',  '/menu/list', '/menu/List', '菜单列表', '', 1, 0, 0),
(3, 'MenuForm',  '/menu/edit', '/menu/Form', '编辑菜单', '', 2, 1, 0),

-- 管理员管理子菜单
(4, 'AdminList', '/admin-mgmt/list', '/admin/List',     '管理员列表', '', 1, 0, 0),
(4, 'AdminEdit', '/admin-mgmt/edit', '/admin/Form',     '编辑管理员', '', 2, 1, 0),
(4, 'RoleList',  '/admin-mgmt/role', '/admin/RoleList', '角色管理',   '', 3, 0, 0),
(4, 'RoleEdit',  '/admin-mgmt/role-edit', '/admin/RoleForm', '编辑角色', '', 4, 1, 0),

-- 系统设置子菜单
(5, 'SystemSetting', '/system/setting', '/system-setting/List', '系统参数', '', 1, 0, 0);

-- 二级菜单（Owner/Agent 页面）
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IsDeleted) VALUES
-- 客服工作台
(6, 'WorkbenchIndex', '/workbench/index', '/workbench/Index', '工作台', '', 1, 0, 0),

-- 平台管理
(7, 'PlatformList', '/platform/list', '/platform/List', '接入平台', '', 1, 0, 0),
(7, 'PlatformForm', '/platform/edit', '/platform/Form', '编辑平台', '', 2, 1, 0),

-- 客服管理（Owner 视角）
(8, 'MyAgentList', '/agent-mgmt/list', '/agent/List', '我的客服', '', 1, 0, 0),
(8, 'AgentForm',   '/agent-mgmt/edit', '/agent/Form', '编辑客服', '', 2, 1, 0),

-- 用户管理
(9, 'ChatUserList', '/user-mgmt/list', '/chat-user/List', '聊天用户', '', 1, 0, 0),

-- 客服配置
(10, 'CsConfigIndex', '/cs-config/index', '/cs-config/Index', '分配规则', '', 1, 0, 0);
