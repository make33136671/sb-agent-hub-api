-- Migration: Phase 3 - Enable feature-flag quotas
-- Run AFTER migration_phase2_quota.sql

-- ========================================
-- 1. Enable Phase 3 feature-flag quota dimensions
-- ========================================
UPDATE SB_QuotaConfig SET IsEnabled = 1 WHERE QuotaKey = 'feature_visitor_form';
UPDATE SB_QuotaConfig SET IsEnabled = 1 WHERE QuotaKey = 'feature_auto_reply';
UPDATE SB_QuotaConfig SET IsEnabled = 1 WHERE QuotaKey = 'feature_satisfaction';
UPDATE SB_QuotaConfig SET IsEnabled = 1 WHERE QuotaKey = 'feature_data_export';

-- ========================================
-- 2. 访客信息收集表单配置
-- ========================================
CREATE TABLE IF NOT EXISTS `SB_VisitorForm` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `OwnerId` INT NOT NULL COMMENT '站长ID',
  `PlatformId` INT NOT NULL DEFAULT 0 COMMENT '平台ID(0=全局)',
  `FormName` VARCHAR(100) NOT NULL DEFAULT '访客信息' COMMENT '表单名称',
  `FormFields` TEXT NOT NULL COMMENT '表单字段配置(JSON)',
  `IsRequired` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否必须填写',
  `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdateTime` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  KEY `idx_owner` (`OwnerId`),
  KEY `idx_platform` (`PlatformId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='访客信息收集表单配置';

-- ========================================
-- 3. 访客表单提交记录
-- ========================================
CREATE TABLE IF NOT EXISTS `SB_VisitorFormSubmission` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `FormId` INT NOT NULL COMMENT '表单ID',
  `OwnerId` INT NOT NULL COMMENT '站长ID',
  `PlatformId` INT NOT NULL DEFAULT 0,
  `ChatUserId` VARCHAR(64) NOT NULL COMMENT '访客ID',
  `FormData` TEXT NOT NULL COMMENT '提交数据(JSON)',
  `IpAddress` VARCHAR(50) DEFAULT '',
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  KEY `idx_form` (`FormId`),
  KEY `idx_owner` (`OwnerId`),
  KEY `idx_chatuser` (`ChatUserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='访客表单提交记录';

-- ========================================
-- 4. 自动回复规则
-- ========================================
CREATE TABLE IF NOT EXISTS `SB_AutoReply` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `OwnerId` INT NOT NULL COMMENT '站长ID',
  `PlatformId` INT NOT NULL DEFAULT 0 COMMENT '平台ID(0=全局)',
  `TriggerType` TINYINT NOT NULL DEFAULT 0 COMMENT '0=关键词 1=首次接入 2=无客服在线',
  `Keywords` VARCHAR(500) DEFAULT '' COMMENT '关键词(逗号分隔，TriggerType=0时)',
  `MatchType` TINYINT NOT NULL DEFAULT 0 COMMENT '0=包含 1=完全匹配 2=正则',
  `ReplyContent` TEXT NOT NULL COMMENT '回复内容',
  `ReplyType` TINYINT NOT NULL DEFAULT 0 COMMENT '0=文本 1=图片 2=富文本',
  `Priority` INT NOT NULL DEFAULT 0 COMMENT '优先级(数字越大越优先)',
  `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdateTime` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  KEY `idx_owner` (`OwnerId`),
  KEY `idx_platform_trigger` (`PlatformId`, `TriggerType`),
  KEY `idx_trigger_type` (`TriggerType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='自动回复规则';

-- ========================================
-- 5. 满意度评价表
-- ========================================
CREATE TABLE IF NOT EXISTS `SB_SatisfactionRating` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `OwnerId` INT NOT NULL COMMENT '站长ID',
  `PlatformId` INT NOT NULL DEFAULT 0,
  `ChatUserId` VARCHAR(64) NOT NULL COMMENT '访客ID',
  `AgentId` INT NOT NULL COMMENT '客服ID',
  `Rating` TINYINT NOT NULL COMMENT '评分 1-5',
  `Tags` VARCHAR(200) DEFAULT '' COMMENT '评价标签(逗号分隔)',
  `Comment` VARCHAR(500) DEFAULT '' COMMENT '评价内容',
  `ConversationId` VARCHAR(128) DEFAULT '' COMMENT '会话ID',
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  KEY `idx_owner` (`OwnerId`),
  KEY `idx_agent` (`AgentId`),
  KEY `idx_chatuser` (`ChatUserId`),
  KEY `idx_create_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='满意度评价表';

-- ========================================
-- 6. 套餐模板表
-- ========================================
CREATE TABLE IF NOT EXISTS `SB_QuotaPackage` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `PackageName` VARCHAR(100) NOT NULL COMMENT '套餐名称',
  `PackageKey` VARCHAR(50) NOT NULL COMMENT '套餐标识',
  `Description` VARCHAR(500) DEFAULT '' COMMENT '套餐描述',
  `Price` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT '价格',
  `PricePeriod` VARCHAR(20) NOT NULL DEFAULT 'month' COMMENT '计费周期(month/year)',
  `QuotaSettings` TEXT NOT NULL COMMENT '配额设置(JSON)',
  `SortOrder` INT NOT NULL DEFAULT 0,
  `IsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdateTime` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  UNIQUE KEY `uk_package_key` (`PackageKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='套餐模板表';

-- ========================================
-- 7. 预设套餐数据
-- ========================================
INSERT IGNORE INTO `SB_QuotaPackage` (`PackageName`, `PackageKey`, `Description`, `Price`, `PricePeriod`, `QuotaSettings`, `SortOrder`) VALUES
('基础版', 'basic', '适合个人站长或小型网站', 0, 'month',
 '{"max_platforms":1,"max_agents":3,"max_concurrent_sessions":10,"max_daily_visitors":100,"message_retention_days":30,"storage_quota_mb":100,"feature_visitor_form":0,"feature_auto_reply":0,"feature_satisfaction":0,"feature_data_export":0}',
 10),
('专业版', 'pro', '适合中型企业，含高级功能', 99, 'month',
 '{"max_platforms":5,"max_agents":10,"max_concurrent_sessions":50,"max_daily_visitors":1000,"message_retention_days":90,"storage_quota_mb":1024,"feature_visitor_form":1,"feature_auto_reply":1,"feature_satisfaction":1,"feature_data_export":0}',
 20),
('企业版', 'enterprise', '适合大型企业，全功能解锁', 299, 'month',
 '{"max_platforms":20,"max_agents":50,"max_concurrent_sessions":200,"max_daily_visitors":10000,"message_retention_days":365,"storage_quota_mb":10240,"feature_visitor_form":1,"feature_auto_reply":1,"feature_satisfaction":1,"feature_data_export":1}',
 30);
