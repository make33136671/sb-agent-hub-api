-- Migration: Create MemberKyc Table

CREATE TABLE IF NOT EXISTS `SB_MemberKyc` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `MemberId` INT NOT NULL,
    `RealName` VARCHAR(100) NULL,
    `IdNumber` VARCHAR(100) NULL,
    `IdFrontImage` VARCHAR(500) NULL,
    `IdBackImage` VARCHAR(500) NULL,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Reviewing',
    `RejectReason` TEXT NULL,
    `SubmitTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ReviewTime` TIMESTAMP NULL,
    `ReviewedBy` INT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX `idx_kyc_member` ON `SB_MemberKyc` (`MemberId`);
