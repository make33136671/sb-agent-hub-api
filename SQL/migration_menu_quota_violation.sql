-- Migration: Add menu entries for Quota Management and Access Violation Log
-- Run AFTER migration_quota_system.sql

-- ========================================
-- 1. 配额管理 (Admin only, top-level)
-- ========================================
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IsDeleted)
VALUES (0, 'QuotaMgmt', '/quota-mgmt', 'BasicLayout', '配额管理', 'lucide:gauge', 6, 0, 0);
SET @quotaMenuId = LAST_INSERT_ID();

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IsDeleted)
VALUES (@quotaMenuId, 'QuotaList', '/quota-mgmt/list', '/quota/List', '配额分配', '', 1, 0, 0);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IsDeleted)
VALUES (@quotaMenuId, 'QuotaDashboard', '/quota-mgmt/dashboard', '/quota/Dashboard', '用量概览', '', 2, 0, 0);

-- ========================================
-- 2. 接入违规日志 (under PlatformMgmt)
-- ========================================
SET @platformMenuId = (SELECT Id FROM sb_agenthub_menu WHERE Name = 'PlatformMgmt' LIMIT 1);

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IsDeleted)
VALUES (@platformMenuId, 'AccessViolationList', '/platform/violations', '/access-violation/List', '接入日志', '', 3, 0, 0);

-- ========================================
-- 3. Associate with super admin role
-- ========================================
SET @superRoleId = (SELECT Id FROM sb_agenthub_admin_role WHERE RoleKey = 'super' LIMIT 1);

INSERT INTO sb_agenthub_admin_role_menu (RoleId, MenuId)
SELECT @superRoleId, Id FROM sb_agenthub_menu
WHERE Name IN ('QuotaMgmt', 'QuotaList', 'QuotaDashboard', 'AccessViolationList')
AND Id NOT IN (SELECT MenuId FROM sb_agenthub_admin_role_menu WHERE RoleId = @superRoleId);
