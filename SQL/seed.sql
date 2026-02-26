-- ========================================
-- 赛博客服系统 种子数据
-- ========================================

-- 初始管理员（密码: admin123）
INSERT INTO SB_Admin (ParentId, Username, Password, RoleId, Name, Status, CreatedTime, IsDeleted) VALUES
(0, 'admin', '$2a$11$abc123hashedpassword', 1, '超级管理员', 0, NOW(), 0);

-- 默认角色
INSERT INTO SB_AdminRole (Name, RoleKey, IsDeleted) VALUES
('超级管理员', 'super', 0),
('运营管理员', 'operator', 0);

-- ========================================
-- 菜单种子数据（Admin 视角）
-- ========================================

-- 一级菜单
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Icon, OrderNo, Status, Type, IsDeleted) VALUES
-- Admin 菜单
(0, '仪表盘', '/dashboard', 'BasicLayout', 'lucide:layout-dashboard', 1, 1, 0, 0),
(0, '会员管理', '/member-mgmt', 'BasicLayout', 'lucide:users', 2, 1, 0, 0),
(0, '菜单管理', '/menu', 'BasicLayout', 'lucide:list-tree', 3, 1, 0, 0),
(0, '管理员管理', '/admin-mgmt', 'BasicLayout', 'lucide:shield', 4, 1, 0, 0),
(0, '系统设置', '/system', 'BasicLayout', 'lucide:settings', 5, 1, 0, 0),

-- Owner 菜单
(0, '客服工作台', '/workbench', 'BasicLayout', 'lucide:headphones', 10, 1, 0, 0),
(0, '平台管理', '/platform', 'BasicLayout', 'lucide:globe', 11, 1, 0, 0),
(0, '客服管理', '/agent-mgmt', 'BasicLayout', 'lucide:user-check', 12, 1, 0, 0),
(0, '用户管理', '/user-mgmt', 'BasicLayout', 'lucide:message-circle', 13, 1, 0, 0),
(0, '客服配置', '/cs-config', 'BasicLayout', 'lucide:sliders', 14, 1, 0, 0);

-- 二级菜单（Admin）
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Icon, OrderNo, Status, Type, IsDeleted) VALUES
(1, '系统概览', '/dashboard/index', '/dashboard/index', '', 1, 1, 1, 0),
(2, '站长管理', '/member-mgmt/owner', '/member/List', '', 1, 1, 1, 0),
(2, '客服管理', '/member-mgmt/agent', '/member/List', '', 2, 1, 1, 0),
(3, '菜单列表', '/menu/list', '/menu/List', '', 1, 1, 1, 0),
(4, '管理员列表', '/admin-mgmt/list', '/admin/List', '', 1, 1, 1, 0),
(4, '角色管理', '/admin-mgmt/role', '/admin/RoleList', '', 2, 1, 1, 0),
(5, '系统参数', '/system/setting', '/system-setting/List', '', 1, 1, 1, 0);

-- 二级菜单（Owner/Agent）
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Icon, OrderNo, Status, Type, IsDeleted) VALUES
(6, '工作台', '/workbench/index', '/workbench/Index', '', 1, 1, 1, 0),
(7, '接入平台', '/platform/list', '/platform/List', '', 1, 1, 1, 0),
(8, '我的客服', '/agent-mgmt/list', '/agent/List', '', 1, 1, 1, 0),
(9, '聊天用户', '/user-mgmt/list', '/chat-user/List', '', 1, 1, 1, 0),
(10, '分配规则', '/cs-config/index', '/cs-config/Index', '', 1, 1, 1, 0);
