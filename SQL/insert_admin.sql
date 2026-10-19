-- ========================================
-- 赛博客服系统 - 初始化管理员数据
-- 用途：数据库表结构由 FreeSql AutoSyncStructure 自动生成后，
--       运行本脚本写入默认角色与初始管理员（可重复执行，幂等）。
-- 初始账号：admin / 111111
-- ========================================

-- 默认角色（不存在则插入）
INSERT INTO sb_agenthub_admin_role (Name, RoleKey, IsDeleted)
SELECT '超级管理员', 'super', 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_admin_role WHERE RoleKey = 'super');

INSERT INTO sb_agenthub_admin_role (Name, RoleKey, IsDeleted)
SELECT '运营管理员', 'operator', 0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_admin_role WHERE RoleKey = 'operator');

-- 初始管理员（密码: 111111，BCrypt 加密，对应 appsettings.json 中 PasswordConfig.StorageMode = Encrypted）
INSERT INTO sb_agenthub_admin (ParentId, Username, Password, RoleId, Name, Avatar, Status, CreatedTime, LastOnlineTime, IsDeleted)
SELECT 0,
       'admin',
       '$2a$11$p7KGZYILT910BSkkevr2q.XqMsvMqFMPCFO1alVPq8TusYZv3KqlW',
       (SELECT Id FROM sb_agenthub_admin_role WHERE RoleKey = 'super'),
       '超级管理员',
       NULL,
       0,
       NOW(),
       NULL,
       0
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_admin WHERE Username = 'admin' AND IsDeleted = 0);
