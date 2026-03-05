-- Migration: Phase 4 - 订单系统
-- Run AFTER migration_phase3_features.sql

-- ========================================
-- 1. 订单表
-- ========================================
CREATE TABLE IF NOT EXISTS `SB_Order` (
  `Id` INT AUTO_INCREMENT PRIMARY KEY,
  `OrderNo` VARCHAR(50) NOT NULL COMMENT '订单号',
  `MemberId` INT NOT NULL COMMENT '站长Id',
  `PackageId` INT NOT NULL COMMENT '套餐Id',
  `PackageName` VARCHAR(100) DEFAULT '' COMMENT '套餐名称(冗余)',
  `OrderType` TINYINT NOT NULL DEFAULT 0 COMMENT '0=新购 1=续费 2=升级 3=降级',
  `Amount` DECIMAL(10,2) NOT NULL DEFAULT 0 COMMENT '订单金额',
  `Currency` VARCHAR(10) DEFAULT 'CNY' COMMENT '货币',
  `Status` TINYINT NOT NULL DEFAULT 0 COMMENT '0=待支付 1=已支付 2=已取消 3=已退款',
  `PaymentMethod` VARCHAR(20) DEFAULT '' COMMENT '支付方式: alipay/wechat/manual',
  `PaymentTime` DATETIME DEFAULT NULL COMMENT '支付时间',
  `PaymentTransactionId` VARCHAR(100) DEFAULT '' COMMENT '第三方支付交易号',
  `EffectiveStartTime` DATETIME DEFAULT NULL COMMENT '生效开始时间',
  `EffectiveEndTime` DATETIME DEFAULT NULL COMMENT '生效结束时间',
  `Remark` VARCHAR(500) DEFAULT '' COMMENT '备注',
  `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdateTime` DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  UNIQUE KEY `uk_order_no` (`OrderNo`),
  KEY `idx_member_id` (`MemberId`),
  KEY `idx_status` (`Status`),
  KEY `idx_create_time` (`CreateTime`)
) COMMENT='订单表';
