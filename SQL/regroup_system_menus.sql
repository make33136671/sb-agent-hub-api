-- 重新分组系统管理菜单：创建二级父菜单并迁移子菜单

-- ========================================
-- 1. 创建二级父菜单（挂载到 System, Id=96）
-- ========================================

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT 96, 'DbCacheMgmt', '/system/db-cache', 'BasicLayout', NULL, '数据库与缓存', 'ion:server-outline', 10, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'DbCacheMgmt' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT 96, 'EmailMgmt', '/system/email', 'BasicLayout', NULL, '邮件管理', 'ion:mail-outline', 11, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'EmailMgmt' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT 96, 'LicenseMgmt', '/system/license-mgmt', 'BasicLayout', NULL, '授权管理', 'ion:shield-checkmark-outline', 12, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'LicenseMgmt' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT 96, 'SysToolMgmt', '/system/sys-tool', 'BasicLayout', NULL, '系统工具', 'ion:build-outline', 13, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'SysToolMgmt' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT 96, 'OpsDeployMgmt', '/system/ops-deploy', 'BasicLayout', NULL, '运维与部署', 'ion:rocket-outline', 14, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'OpsDeployMgmt' AND "IsDeleted" = false);

-- ========================================
-- 2. 迁移子菜单到新的父菜单下
-- ========================================

-- 数据库与缓存
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'DbCacheMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 1
WHERE "Name" = 'Database' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'DbCacheMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 2
WHERE "Name" = 'Cache' AND "IsDeleted" = false;

-- 邮件管理
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'EmailMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 1
WHERE "Name" = 'EmailConfig' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'EmailMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 2
WHERE "Name" = 'EmailLog' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'EmailMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 3
WHERE "Name" = 'EmailTemplate' AND "IsDeleted" = false;

-- 授权管理
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'LicenseMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 1
WHERE "Name" = 'LicenseStatus' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'LicenseMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 2
WHERE "Name" = 'LicenseManage' AND "IsDeleted" = false;

-- 系统工具
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'SysToolMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 1
WHERE "Name" = 'SqlTool' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'SysToolMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 2
WHERE "Name" = 'AppSettings' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'SysToolMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 3
WHERE "Name" = 'AccessToken' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'SysToolMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 4
WHERE "Name" = 'RealName' AND "IsDeleted" = false;

-- 运维与部署
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'OpsDeployMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 1
WHERE "Name" = 'BackgroundTask' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'OpsDeployMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 2
WHERE "Name" = 'LogViewer' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'OpsDeployMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 3
WHERE "Name" = 'BackupReceive' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'OpsDeployMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 4
WHERE "Name" = 'Deploy' AND "IsDeleted" = false;
UPDATE sb_agenthub_menu SET "ParentId" = (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'OpsDeployMgmt' AND "IsDeleted" = false LIMIT 1), "OrderNo" = 5
WHERE "Name" = 'OpsTools' AND "IsDeleted" = false;

-- ========================================
-- 3. 将新父菜单关联到超级管理员角色
-- ========================================
INSERT INTO sb_agenthub_admin_role_menu ("RoleId", "MenuId")
SELECT r."Id", m."Id"
FROM sb_agenthub_admin_role r, sb_agenthub_menu m
WHERE r."RoleKey" = 'super'
  AND m."IsDeleted" = false
  AND m."Name" IN ('DbCacheMgmt', 'EmailMgmt', 'LicenseMgmt', 'SysToolMgmt', 'OpsDeployMgmt')
  AND NOT EXISTS (SELECT 1 FROM sb_agenthub_admin_role_menu rm WHERE rm."RoleId" = r."Id" AND rm."MenuId" = m."Id");
