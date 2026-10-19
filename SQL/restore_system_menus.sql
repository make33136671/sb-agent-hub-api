-- 恢复 sb 框架系统管理和维护功能菜单（幂等，可重复执行）
-- 挂载到 System（系统管理, Id=96）下作为子菜单

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'Database', '/system/database', '/database/List', NULL, '数据库管理', 'ion:server-outline', 10, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'Database' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'Cache', '/system/cache', '/cache/List', NULL, '缓存管理', 'ion:server-outline', 11, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'Cache' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'BackgroundTask', '/system/background-task', '/background-task/List', NULL, '后台任务', 'ion:timer-outline', 12, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'BackgroundTask' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'LogViewer', '/system/log-viewer', '/log-viewer/List', NULL, '日志查看', 'ion:document-text-outline', 13, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'LogViewer' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'SqlTool', '/system/sql', '/sql/List', NULL, 'SQL工具', 'ion:code-slash-outline', 14, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'SqlTool' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'AppSettings', '/system/app-settings', '/app-settings/index', NULL, '应用设置', 'ion:apps-outline', 15, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'AppSettings' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'AccessToken', '/system/access-token', '/access-token/index', NULL, 'API令牌', 'ion:key-outline', 16, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'AccessToken' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'RealName', '/system/real-name', '/real-name/List', NULL, '实名认证', 'ion:card-outline', 17, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'RealName' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'BackupReceive', '/system/backup-receive', '/backup-receive/ProductList', NULL, '备份接收', 'ion:cloud-download-outline', 18, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'BackupReceive' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'Deploy', '/system/deploy', '/deploy/List', NULL, '部署管理', 'ion:cloud-upload-outline', 19, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'Deploy' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'OpsTools', '/system/ops-tools', '/ops-tools/index', NULL, '运维工具', 'ion:construct-outline', 20, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'OpsTools' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'EmailConfig', '/system/email/config', '/email/config/index', NULL, '邮件配置', 'ion:mail-outline', 21, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'EmailConfig' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'EmailLog', '/system/email/log', '/email/log/index', NULL, '邮件日志', 'ion:mail-unread-outline', 22, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'EmailLog' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'EmailTemplate', '/system/email/template', '/email/template/index', NULL, '邮件模板', 'ion:mail-open-outline', 23, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'EmailTemplate' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'LicenseStatus', '/system/license', '/license/Status', NULL, '授权状态', 'ion:shield-checkmark-outline', 24, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'LicenseStatus' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_menu ("ParentId", "Name", "Path", "Component", "Redirect", "Title", "Icon", "OrderNo", "HideInMenu", "IgnoreKeepAlive", "IsDeleted")
SELECT (SELECT "Id" FROM sb_agenthub_menu WHERE "Name" = 'System' AND "IsDeleted" = false LIMIT 1), 'LicenseManage', '/system/license-manage', '/license-manage/ClientList', NULL, '授权管理', 'ion:key-outline', 25, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_menu WHERE "Name" = 'LicenseManage' AND "IsDeleted" = false);

INSERT INTO sb_agenthub_admin_role_menu ("RoleId", "MenuId")
SELECT r."Id", m."Id"
FROM sb_agenthub_admin_role r, sb_agenthub_menu m
WHERE r."RoleKey" = 'super'
  AND m."IsDeleted" = false
  AND m."Name" IN (
    'Database', 'Cache', 'BackgroundTask', 'LogViewer', 'SqlTool',
    'AppSettings', 'AccessToken', 'RealName', 'BackupReceive', 'Deploy',
    'OpsTools', 'EmailConfig', 'EmailLog', 'EmailTemplate',
    'LicenseStatus', 'LicenseManage'
  )
  AND NOT EXISTS (SELECT 1 FROM sb_agenthub_admin_role_menu rm WHERE rm."RoleId" = r."Id" AND rm."MenuId" = m."Id");
