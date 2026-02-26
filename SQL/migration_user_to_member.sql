-- Migration: Rename SB_User to SB_Member
-- Run this BEFORE starting the updated application

-- 1. Rename the table
RENAME TABLE `SB_User` TO `SB_Member`;

-- 2. Add new columns to SB_Member
ALTER TABLE `SB_Member`
    ADD COLUMN `PaymentPassword` VARCHAR(200) NULL AFTER `LastOnlineTime`,
    ADD COLUMN `CreditScore` INT NOT NULL DEFAULT 100 AFTER `PaymentPassword`,
    ADD COLUMN `RealNameStatus` INT NOT NULL DEFAULT 0 AFTER `CreditScore`,
    ADD COLUMN `InvitationCode` VARCHAR(50) NULL AFTER `RealNameStatus`,
    ADD COLUMN `ParentId` INT NULL AFTER `InvitationCode`,
    ADD COLUMN `AncestryPath` VARCHAR(500) NULL AFTER `ParentId`;

-- 3. Drop columns that are not needed in base (UserType, Phone)
ALTER TABLE `SB_Member`
    DROP COLUMN IF EXISTS `UserType`,
    DROP COLUMN IF EXISTS `Phone`;

-- 4. Change Status column to VARCHAR for enum mapping
ALTER TABLE `SB_Member` MODIFY COLUMN `Status` VARCHAR(50) NOT NULL DEFAULT 'Active';

-- 5. Add indexes
CREATE INDEX `idx_member_invitation_code` ON `SB_Member` (`InvitationCode`);
CREATE INDEX `idx_member_parent_id` ON `SB_Member` (`ParentId`);
CREATE INDEX `idx_member_ancestry_path` ON `SB_Member` (`AncestryPath`(255));
