-- =============================================================
-- Migration G: saibo 平台表名迁移（Phase J1）
-- SB_* (PascalCase) → sb_agenthub_* (snake_case)
-- 遵循 saibo 平台 sb_<app>_ 表前缀规范（决策 D35）
-- 创建：2026-08-20
-- 说明：
--   1. 覆盖 28 张实体管理表 + 5 张 SB_ 前缀历史遗留表
--   2. SYS_BackgroundTaskLog（非 SB_ 前缀）保持不变
--   3. chat_rt_*（框架 Realtime 表，归属 Sb.Framework.Chat.Realtime）保持不变
--   4. 执行前请确保应用已停止（避免写入旧表名）
-- =============================================================

-- ---------- 实体管理表（28 张） ----------
RENAME TABLE `SB_AccessViolationLog` TO `sb_agenthub_access_violation_log`;
RENAME TABLE `SB_AdminRoleMenu` TO `sb_agenthub_admin_role_menu`;
RENAME TABLE `SB_AdminRole` TO `sb_agenthub_admin_role`;
RENAME TABLE `SB_Admin` TO `sb_agenthub_admin`;
RENAME TABLE `SB_AutoReply` TO `sb_agenthub_auto_reply`;
RENAME TABLE `SB_ChatPlatform` TO `sb_agenthub_chat_platform`;
RENAME TABLE `SB_ChatUserLoginRecord` TO `sb_agenthub_chat_user_login_record`;
RENAME TABLE `SB_ChatUser` TO `sb_agenthub_chat_user`;
RENAME TABLE `SB_ClientErrorLog` TO `sb_agenthub_client_error_log`;
RENAME TABLE `SB_CustomerServiceConfig` TO `sb_agenthub_customer_service_config`;
RENAME TABLE `SB_FileRecord` TO `sb_agenthub_file_record`;
RENAME TABLE `SB_LoginLog` TO `sb_agenthub_login_log`;
RENAME TABLE `SB_MemberQuota` TO `sb_agenthub_member_quota`;
RENAME TABLE `SB_Member` TO `sb_agenthub_member`;
RENAME TABLE `SB_Menu` TO `sb_agenthub_menu`;
RENAME TABLE `SB_MigrationVersion` TO `sb_agenthub_migration_version`;
RENAME TABLE `SB_Order` TO `sb_agenthub_order`;
RENAME TABLE `SB_PendingDomain` TO `sb_agenthub_pending_domain`;
RENAME TABLE `SB_QuotaConfig` TO `sb_agenthub_quota_config`;
RENAME TABLE `SB_QuotaPackage` TO `sb_agenthub_quota_package`;
RENAME TABLE `SB_QuotaUsage` TO `sb_agenthub_quota_usage`;
RENAME TABLE `SB_SatisfactionRating` TO `sb_agenthub_satisfaction_rating`;
RENAME TABLE `SB_SystemSetting_Category` TO `sb_agenthub_system_setting_category`;
RENAME TABLE `SB_SystemSetting` TO `sb_agenthub_system_setting`;
RENAME TABLE `SB_UserPreferences` TO `sb_agenthub_user_preferences`;
RENAME TABLE `SB_VisitorFormSubmission` TO `sb_agenthub_visitor_form_submission`;
RENAME TABLE `SB_VisitorForm` TO `sb_agenthub_visitor_form`;
RENAME TABLE `SB_VisitorQueue` TO `sb_agenthub_visitor_queue`;

-- ---------- 历史遗留表（5 张，SB_ 前缀） ----------
RENAME TABLE `SB_EmailConfig` TO `sb_agenthub_email_config`;
RENAME TABLE `SB_EmailLog` TO `sb_agenthub_email_log`;
RENAME TABLE `SB_EmailTemplate` TO `sb_agenthub_email_template`;
RENAME TABLE `SB_EmailVerificationCode` TO `sb_agenthub_email_verification_code`;
RENAME TABLE `SB_MemberKyc` TO `sb_agenthub_member_kyc`;

-- ---------- 注册迁移版本（表已重命名，使用新表名） ----------
INSERT INTO `sb_agenthub_migration_version` (`Version`, `Description`, `AppliedAt`)
VALUES ('G-saibo-rename', 'saibo 平台迁移：SB_* → sb_agenthub_*（33 张表）', now());
