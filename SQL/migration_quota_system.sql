-- ============================================================
-- 配额系统迁移脚本
-- 日期: 2026-03-04
-- 说明: 一期 - 配额系统基础 + 平台接入方式管理
-- ============================================================

-- 1. 扩展 sb_agenthub_chat_platform 表
ALTER TABLE `sb_agenthub_chat_platform`
  ADD COLUMN IF NOT EXISTS `AccessType` TINYINT NOT NULL DEFAULT 0 COMMENT '接入方式: 0=JS 1=Vue 2=Chat' AFTER `PlatformType`,
  ADD COLUMN IF NOT EXISTS `Logo` VARCHAR(500) DEFAULT '' COMMENT '平台Logo URL' AFTER `PlatformName`,
  ADD COLUMN IF NOT EXISTS `AllowedDomains` TEXT DEFAULT NULL COMMENT '允许的域名列表(JSON数组)' AFTER `Domain`;

-- 2. 创建配额定义表
CREATE TABLE IF NOT EXISTS `sb_agenthub_quota_config` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `QuotaKey` VARCHAR(50) NOT NULL COMMENT '配额标识，如 max_platforms',
  `QuotaName` VARCHAR(100) NOT NULL COMMENT '显示名称',
  `QuotaType` TINYINT NOT NULL DEFAULT 0 COMMENT '0=数量型 1=开关型 2=存储型',
  `DefaultValue` INT NOT NULL DEFAULT 0 COMMENT '默认值',
  `MinValue` INT NOT NULL DEFAULT 0,
  `MaxValue` INT NOT NULL DEFAULT 999999,
  `Unit` VARCHAR(20) DEFAULT '' COMMENT '单位（个、MB、天）',
  `Description` VARCHAR(500) DEFAULT '',
  `SortOrder` INT NOT NULL DEFAULT 0,
  `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  UNIQUE KEY `uk_quota_key` (`QuotaKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='配额定义表';

-- 3. 创建站长配额分配表
CREATE TABLE IF NOT EXISTS `sb_agenthub_member_quota` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `MemberId` INT NOT NULL COMMENT '站长Id',
  `QuotaKey` VARCHAR(50) NOT NULL COMMENT '配额标识',
  `QuotaValue` INT NOT NULL DEFAULT 0,
  `GrantedBy` INT DEFAULT NULL COMMENT '授权管理员Id',
  `GrantedByType` VARCHAR(10) DEFAULT 'Admin' COMMENT 'Admin/System',
  `ExpiresAt` DATETIME DEFAULT NULL COMMENT '过期时间（四期用）',
  `Remark` VARCHAR(200) DEFAULT '',
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdateTime` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  UNIQUE KEY `uk_member_quota` (`MemberId`, `QuotaKey`),
  KEY `idx_member_id` (`MemberId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='站长配额分配表';

-- 4. 创建非法接入记录表
CREATE TABLE IF NOT EXISTS `sb_agenthub_access_violation_log` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `PlatformId` INT NOT NULL,
  `PlatformKey` VARCHAR(64) NOT NULL,
  `OwnerId` INT NOT NULL DEFAULT 0,
  `RequestDomain` VARCHAR(200) DEFAULT '',
  `RequestSource` VARCHAR(20) NOT NULL COMMENT 'js/vue/chat',
  `ExpectedSource` VARCHAR(20) NOT NULL,
  `Reason` VARCHAR(50) NOT NULL COMMENT 'domain_mismatch/type_mismatch/platform_disabled',
  `IpAddress` VARCHAR(50) DEFAULT '',
  `UserAgent` VARCHAR(500) DEFAULT '',
  `RequestUrl` VARCHAR(500) DEFAULT '',
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  KEY `idx_platform_id` (`PlatformId`),
  KEY `idx_owner_id` (`OwnerId`),
  KEY `idx_create_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='非法接入记录表';

-- 5. 创建配额用量表（二期启用）
CREATE TABLE IF NOT EXISTS `sb_agenthub_quota_usage` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `MemberId` INT NOT NULL,
  `QuotaKey` VARCHAR(50) NOT NULL,
  `CurrentUsage` INT NOT NULL DEFAULT 0,
  `UpdateTime` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  UNIQUE KEY `uk_member_usage` (`MemberId`, `QuotaKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='配额实时用量表';

-- 6. 种子数据 - 一期配额定义
INSERT IGNORE INTO `sb_agenthub_quota_config` (`QuotaKey`, `QuotaName`, `QuotaType`, `DefaultValue`, `MinValue`, `MaxValue`, `Unit`, `Description`, `SortOrder`, `IsEnabled`) VALUES
('max_platforms', '最大平台数', 0, 1, 0, 100, '个', '可创建的接入平台数量上限', 10, 1),
('max_agents', '最大客服坐席数', 0, 3, 0, 500, '个', '可启用的客服人员数量上限', 20, 1);

-- 7. 种子数据 - 二期配额定义（先插入但不启用）
INSERT IGNORE INTO `sb_agenthub_quota_config` (`QuotaKey`, `QuotaName`, `QuotaType`, `DefaultValue`, `MinValue`, `MaxValue`, `Unit`, `Description`, `SortOrder`, `IsEnabled`) VALUES
('max_concurrent_sessions', '最大并发会话数', 0, 10, 1, 1000, '个', '单个客服同时接待的访客上限', 30, 0),
('max_daily_visitors', '每日访客会话数', 0, 100, 0, 100000, '次', '平台每日最多接待访客数', 40, 0),
('message_retention_days', '消息保留天数', 0, 30, 7, 3650, '天', '聊天记录保留时长', 50, 0),
('storage_quota_mb', '文件存储容量', 2, 100, 0, 102400, 'MB', '聊天文件和图片的存储上限', 60, 0);

-- 8. 种子数据 - 三期配额定义（先插入但不启用）
INSERT IGNORE INTO `sb_agenthub_quota_config` (`QuotaKey`, `QuotaName`, `QuotaType`, `DefaultValue`, `MinValue`, `MaxValue`, `Unit`, `Description`, `SortOrder`, `IsEnabled`) VALUES
('feature_visitor_form', '访客信息收集', 1, 0, 0, 1, '', '聊天前弹出表单收集访客信息', 100, 0),
('feature_auto_reply', '自动回复/机器人', 1, 0, 0, 1, '', '是否启用自动回复功能', 110, 0),
('feature_satisfaction', '满意度评价', 1, 0, 0, 1, '', '聊天结束后弹出评价', 120, 0),
('feature_data_export', '数据导出', 1, 0, 0, 1, '', '是否允许导出聊天记录', 130, 0);
