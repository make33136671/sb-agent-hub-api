-- ========================================
-- 修复系统菜单路径冲突
-- 问题：框架 System 菜单与客服系统 SystemMgmt 菜单都使用 /system 路径，
--       导致前端路由冲突，部署管理等子菜单无法正常显示
-- 修复：将框架 System 菜单及其所有子菜单路径从 /system 改为 /sys-admin
-- ========================================

-- 1. 更新一级菜单：System (系统管理)
UPDATE sb_agenthub_menu
SET Path = '/sys-admin'
WHERE Name = 'System' AND ParentId = 0 AND IsDeleted = 0;

-- 2. 更新二级菜单：数据库与缓存、邮件管理、授权管理、系统工具、运维与部署
UPDATE sb_agenthub_menu
SET Path = '/sys-admin/db-cache'
WHERE Name = 'DbCacheMgmt' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/database'
WHERE Name = 'Database' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/cache'
WHERE Name = 'Cache' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/email'
WHERE Name = 'EmailMgmt' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/email/config'
WHERE Name = 'EmailConfig' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/email/log'
WHERE Name = 'EmailLog' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/email/template'
WHERE Name = 'EmailTemplate' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/license-mgmt'
WHERE Name = 'LicenseMgmt' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/license'
WHERE Name = 'LicenseStatus' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/license-manage'
WHERE Name = 'LicenseManage' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/sys-tool'
WHERE Name = 'SysToolMgmt' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/sql'
WHERE Name = 'SqlTool' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/app-settings'
WHERE Name = 'AppSettings' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/access-token'
WHERE Name = 'AccessToken' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/real-name'
WHERE Name = 'RealName' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/ops-deploy'
WHERE Name = 'OpsDeployMgmt' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/background-task'
WHERE Name = 'BackgroundTask' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/log-viewer'
WHERE Name = 'LogViewer' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/backup-receive'
WHERE Name = 'BackupReceive' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/deploy'
WHERE Name = 'Deploy' AND IsDeleted = 0;

UPDATE sb_agenthub_menu
SET Path = '/sys-admin/ops-tools'
WHERE Name = 'OpsTools' AND IsDeleted = 0;

-- ========================================
-- 验证：查询修改后的系统管理菜单树
-- ========================================
-- SELECT Id, Name, Path, Title, ParentId, OrderNo
-- FROM sb_agenthub_menu
-- WHERE Path LIKE '/sys-admin%' AND IsDeleted = 0
-- ORDER BY ParentId, OrderNo;

-- ========================================
-- 同时建议：清除菜单缓存（如果使用了 Redis 缓存）
-- 方法1：重启 API 服务（内存缓存会自动清空）
-- 方法2：在 Redis 中执行：FLUSHDB 或删除 menu: 开头的 key
-- ========================================
