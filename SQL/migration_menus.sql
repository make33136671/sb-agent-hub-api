-- Migration: Insert new menu entries for email, background task, login log, member management
-- Run this AFTER the tables are created
-- NOTE: Adjust ParentId values based on your existing menu structure

-- Find the System menu ID
SET @systemMenuId = (SELECT Id FROM SB_Menu WHERE Name = 'System' LIMIT 1);

-- Member Management (top level)
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, IsDeleted)
VALUES (0, 'Member', '/member', 'LAYOUT', '会员管理', 'ion:people-outline', 10, 0);
SET @memberMenuId = LAST_INSERT_ID();

INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, IsDeleted, HideInMenu) VALUES
(@memberMenuId, 'MemberList', '/member/list', '/member/List', '会员列表', 'ion:person-outline', 1, 0, 0),
(@memberMenuId, 'MemberForm', '/member/form', '/member/Form', '编辑会员', '', 3, 0, 1),
(@memberMenuId, 'MemberTree', '/member/tree', '/member/Tree', '推荐树', 'ion:git-branch-outline', 2, 0, 0);

-- Email Management (top level)
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, IsDeleted)
VALUES (0, 'Email', '/email', 'LAYOUT', '邮件管理', 'ion:mail-outline', 80, 0);
SET @emailMenuId = LAST_INSERT_ID();

INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, IsDeleted) VALUES
(@emailMenuId, 'EmailConfig', '/email/config', '/email/config/index', '邮件配置', 'ion:settings-outline', 1, 0),
(@emailMenuId, 'EmailTemplate', '/email/template', '/email/template/index', '邮件模板', 'ion:document-text-outline', 2, 0),
(@emailMenuId, 'EmailLog', '/email/log', '/email/log/index', '发送日志', 'ion:list-outline', 3, 0);

-- System sub-menus (background task, system log, login log)
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, IsDeleted) VALUES
(@systemMenuId, 'BackgroundTask', '/system/background-task', '/background-task/List', '后台任务', 'ion:timer-outline', 5, 0),
(@systemMenuId, 'SystemLog', '/system/log', '/background-task/Log', '系统日志', 'ion:document-outline', 6, 0),
(@systemMenuId, 'LoginLog', '/system/login-log', '/login-log/List', '登录日志', 'ion:log-in-outline', 7, 0);

-- Associate new menus with super admin role
SET @superRoleId = (SELECT Id FROM SB_AdminRole WHERE RoleKey = 'super' LIMIT 1);

INSERT INTO SB_AdminRoleMenu (RoleId, MenuId)
SELECT @superRoleId, Id FROM SB_Menu
WHERE Name IN ('Member', 'MemberList', 'MemberForm', 'MemberTree', 'Email', 'EmailConfig', 'EmailTemplate', 'EmailLog', 'BackgroundTask', 'SystemLog', 'LoginLog')
AND Id NOT IN (SELECT MenuId FROM SB_AdminRoleMenu WHERE RoleId = @superRoleId);
