-- Migration: Create Login Log Table

CREATE TABLE IF NOT EXISTS `sb_agenthub_login_log` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NOT NULL,
    `UserType` INT NOT NULL DEFAULT 0 COMMENT '0=H5会员, 1=Admin管理员',
    `Username` VARCHAR(100) NULL,
    `IpAddress` VARCHAR(50) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `Status` INT NOT NULL DEFAULT 0 COMMENT '0=成功, 1=失败',
    `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX `idx_login_log_user` ON `sb_agenthub_login_log` (`UserId`, `UserType`);
CREATE INDEX `idx_login_log_time` ON `sb_agenthub_login_log` (`CreateTime`);
