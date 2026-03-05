-- Migration: Phase 2 - Enable quota dimensions for session control & storage
-- Run AFTER migration_quota_system.sql

-- ========================================
-- 1. Enable Phase 2 quota dimensions
-- ========================================
UPDATE SB_QuotaConfig SET IsEnabled = 1 WHERE QuotaKey = 'max_concurrent_sessions';
UPDATE SB_QuotaConfig SET IsEnabled = 1 WHERE QuotaKey = 'max_daily_visitors';
UPDATE SB_QuotaConfig SET IsEnabled = 1 WHERE QuotaKey = 'message_retention_days';
UPDATE SB_QuotaConfig SET IsEnabled = 1 WHERE QuotaKey = 'storage_quota_mb';

-- ========================================
-- 2. Visitor queue table
-- ========================================
CREATE TABLE IF NOT EXISTS `SB_VisitorQueue` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `ChatUserId` VARCHAR(64) NOT NULL COMMENT 'OpenIM 用户ID',
  `PlatformId` INT NOT NULL COMMENT '平台ID',
  `OwnerId` INT NOT NULL COMMENT '站长ID',
  `QueuePosition` INT NOT NULL COMMENT '排队位置',
  `EnqueueTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '入队时间',
  `Status` TINYINT NOT NULL DEFAULT 0 COMMENT '0=等待 1=已分配 2=已取消 3=已超时',
  `AssignedAgentId` INT DEFAULT NULL COMMENT '分配的客服ID',
  `AssignedTime` DATETIME DEFAULT NULL COMMENT '分配时间',
  `CancelTime` DATETIME DEFAULT NULL COMMENT '取消时间',
  KEY `idx_owner_status` (`OwnerId`, `Status`),
  KEY `idx_platform_status` (`PlatformId`, `Status`),
  KEY `idx_chatuser` (`ChatUserId`)
) COMMENT='访客排队表';

-- ========================================
-- 3. File upload record table (for storage tracking)
-- ========================================
CREATE TABLE IF NOT EXISTS `SB_FileRecord` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `OwnerId` INT NOT NULL COMMENT '所属站长ID',
  `PlatformId` INT NOT NULL DEFAULT 0 COMMENT '所属平台ID',
  `UploaderId` VARCHAR(64) DEFAULT '' COMMENT '上传者ID(ChatUserId或MemberId)',
  `UploaderType` VARCHAR(10) DEFAULT 'visitor' COMMENT 'visitor/agent',
  `FileName` VARCHAR(200) NOT NULL COMMENT '原始文件名',
  `StoredPath` VARCHAR(500) NOT NULL COMMENT '存储路径',
  `FileSize` BIGINT NOT NULL DEFAULT 0 COMMENT '文件大小(字节)',
  `FileType` VARCHAR(20) DEFAULT '' COMMENT '文件类型(image/file/voice)',
  `MimeType` VARCHAR(100) DEFAULT '' COMMENT 'MIME类型',
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  KEY `idx_owner` (`OwnerId`),
  KEY `idx_platform` (`PlatformId`),
  KEY `idx_create_time` (`CreateTime`)
) COMMENT='文件上传记录表(用于存储配额追踪)';
