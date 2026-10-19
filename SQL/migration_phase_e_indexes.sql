-- ============================================================
-- Migration: Phase E — Database Redesign
-- Add missing indexes, fix FileRecord defaults, create migration version table
-- Run AFTER migration_realtime_rename.sql, BEFORE starting the app
-- ============================================================

-- 1. Create migration version tracking table
CREATE TABLE IF NOT EXISTS `sb_agenthub_migration_version` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Version` VARCHAR(50) NOT NULL,
    `Description` VARCHAR(200) DEFAULT NULL,
    `AppliedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE INDEX `ix_migrationversion_version` (`Version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. sb_agenthub_chat_user — add missing indexes
CREATE INDEX `ix_chatuser_platformid` ON `sb_agenthub_chat_user` (`PlatformId`);
CREATE INDEX `ix_chatuser_ownerid` ON `sb_agenthub_chat_user` (`OwnerId`);
CREATE INDEX `ix_chatuser_assignedagentid` ON `sb_agenthub_chat_user` (`AssignedAgentId`);
CREATE INDEX `ix_chatuser_groupid` ON `sb_agenthub_chat_user` (`GroupId`);
CREATE INDEX `ix_chatuser_clientid` ON `sb_agenthub_chat_user` (`ClientId`);
CREATE INDEX `ix_chatuser_status` ON `sb_agenthub_chat_user` (`Status`, `OnlineStatus`);

-- 3. sb_agenthub_member — add missing indexes
CREATE UNIQUE INDEX `ix_member_username` ON `sb_agenthub_member` (`Username`);
CREATE INDEX `ix_member_realtimeuserid` ON `sb_agenthub_member` (`RealtimeUserId`);
CREATE INDEX `ix_member_ownerid` ON `sb_agenthub_member` (`OwnerId`);
CREATE INDEX `ix_member_role_status` ON `sb_agenthub_member` (`Role`, `Status`);

-- 4. sb_agenthub_chat_platform — add missing index
CREATE INDEX `ix_chatplatform_ownerid` ON `sb_agenthub_chat_platform` (`OwnerId`);

-- 5. sb_agenthub_order — add missing indexes
CREATE INDEX `ix_order_memberid` ON `sb_agenthub_order` (`MemberId`);
CREATE INDEX `ix_order_status` ON `sb_agenthub_order` (`Status`);
CREATE INDEX `ix_order_createtime` ON `sb_agenthub_order` (`CreateTime`);

-- 6. sb_agenthub_visitor_queue — add missing indexes
CREATE INDEX `ix_visitorqueue_chatuserid` ON `sb_agenthub_visitor_queue` (`ChatUserId`);
CREATE INDEX `ix_visitorqueue_platform_owner` ON `sb_agenthub_visitor_queue` (`PlatformId`, `OwnerId`);
CREATE INDEX `ix_visitorqueue_status` ON `sb_agenthub_visitor_queue` (`Status`);

-- 7. sb_agenthub_chat_user_login_record — add missing indexes
CREATE INDEX `ix_chatuserloginrecord_chatuserdbid` ON `sb_agenthub_chat_user_login_record` (`ChatUserDbId`);
CREATE INDEX `ix_chatuserloginrecord_platformid` ON `sb_agenthub_chat_user_login_record` (`PlatformId`);
CREATE INDEX `ix_chatuserloginrecord_logintime` ON `sb_agenthub_chat_user_login_record` (`LoginTime`);

-- 8. sb_agenthub_auto_reply — add missing indexes
CREATE INDEX `ix_autoreply_ownerid_platformid` ON `sb_agenthub_auto_reply` (`OwnerId`, `PlatformId`);
CREATE INDEX `ix_autoreply_isenabled` ON `sb_agenthub_auto_reply` (`IsEnabled`);

-- 9. sb_agenthub_satisfaction_rating — add missing indexes
CREATE INDEX `ix_satisfactionrating_chatuserid` ON `sb_agenthub_satisfaction_rating` (`ChatUserId`);
CREATE INDEX `ix_satisfactionrating_agentid` ON `sb_agenthub_satisfaction_rating` (`AgentId`);
CREATE INDEX `ix_satisfactionrating_platform_owner` ON `sb_agenthub_satisfaction_rating` (`PlatformId`, `OwnerId`);

-- 10. sb_agenthub_file_record — add missing indexes + fix CreateTime default
CREATE INDEX `ix_filerecord_ownerid_platformid` ON `sb_agenthub_file_record` (`OwnerId`, `PlatformId`);
CREATE INDEX `ix_filerecord_uploaderid` ON `sb_agenthub_file_record` (`UploaderId`);
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0;

-- 11. sb_agenthub_visitor_form — add missing index
CREATE INDEX `ix_visitorform_platformid_ownerid` ON `sb_agenthub_visitor_form` (`PlatformId`, `OwnerId`);

-- 12. sb_agenthub_visitor_form_submission — add missing indexes
CREATE INDEX `ix_visitorformsubmission_formid` ON `sb_agenthub_visitor_form_submission` (`FormId`);
CREATE INDEX `ix_visitorformsubmission_chatuserid` ON `sb_agenthub_visitor_form_submission` (`ChatUserId`);

-- 13. sb_agenthub_user_preferences — add unique index
CREATE UNIQUE INDEX `ix_userpreferences_userid_usertype` ON `sb_agenthub_user_preferences` (`UserId`, `UserType`);

-- 14. sb_agenthub_customer_service_config — add unique index
CREATE UNIQUE INDEX `ix_customerserviceconfig_ownerid` ON `sb_agenthub_customer_service_config` (`OwnerId`);

-- 15. sb_agenthub_login_log — add missing indexes
CREATE INDEX `ix_loginlog_userid_usertype` ON `sb_agenthub_login_log` (`UserId`, `UserType`);
CREATE INDEX `ix_loginlog_createtime` ON `sb_agenthub_login_log` (`CreateTime`);

-- 16. sb_agenthub_access_violation_log — add missing indexes
CREATE INDEX `ix_accessviolation_platformid` ON `sb_agenthub_access_violation_log` (`PlatformId`);
CREATE INDEX `ix_accessviolation_createtime` ON `sb_agenthub_access_violation_log` (`CreateTime`);

-- 17. sb_agenthub_client_error_log — add missing indexes
CREATE INDEX `ix_clienterrorlog_userid` ON `sb_agenthub_client_error_log` (`UserId`);
CREATE INDEX `ix_clienterrorlog_createtime` ON `sb_agenthub_client_error_log` (`CreateTime`);

-- 18. Record this migration
INSERT INTO `sb_agenthub_migration_version` (`Version`, `Description`) VALUES
    ('20260820_01', 'Phase E: Added missing indexes, fixed FileRecord defaults, created migration version table');
