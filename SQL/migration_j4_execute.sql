-- =============================================================
-- J4 执行脚本：将残留 sb_* 表重命名为 sb_agenthub_*
-- MySQL 不支持 RENAME TABLE IF EXISTS，逐条执行容错
-- 执行日期：2026-08-21
-- =============================================================

RENAME TABLE `sb_admin` TO `sb_agenthub_admin`;
RENAME TABLE `sb_adminrole` TO `sb_agenthub_admin_role`;
RENAME TABLE `sb_adminrolemenu` TO `sb_agenthub_admin_role_menu`;
RENAME TABLE `sb_autoreply` TO `sb_agenthub_auto_reply`;
RENAME TABLE `sb_clienterrorlog` TO `sb_agenthub_client_error_log`;
RENAME TABLE `sb_loginlog` TO `sb_agenthub_login_log`;
RENAME TABLE `sb_menu` TO `sb_agenthub_menu`;
RENAME TABLE `sb_systemsetting` TO `sb_agenthub_system_setting`;
RENAME TABLE `sb_systemsetting_category` TO `sb_agenthub_system_setting_category`;
RENAME TABLE `sb_visitorqueue` TO `sb_agenthub_visitor_queue`;
RENAME TABLE `sb_satisfactionrating` TO `sb_agenthub_satisfaction_rating`;
RENAME TABLE `sb_visitorform` TO `sb_agenthub_visitor_form`;
RENAME TABLE `sb_visitorformsubmission` TO `sb_agenthub_visitor_form_submission`;
RENAME TABLE `sb_userpreferences` TO `sb_agenthub_user_preferences`;
RENAME TABLE `sb_pendingdomain` TO `sb_agenthub_pending_domain`;
RENAME TABLE `sb_order` TO `sb_agenthub_order`;
RENAME TABLE `sb_quotapackage` TO `sb_agenthub_quota_package`;
RENAME TABLE `sb_quotausage` TO `sb_agenthub_quota_usage`;
RENAME TABLE `sb_emailconfig` TO `sb_agenthub_email_config`;
RENAME TABLE `sb_emaillog` TO `sb_agenthub_email_log`;
RENAME TABLE `sb_emailtemplate` TO `sb_agenthub_email_template`;
RENAME TABLE `sb_emailverificationcode` TO `sb_agenthub_email_verification_code`;
RENAME TABLE `sb_memberkyc` TO `sb_agenthub_member_kyc`;
RENAME TABLE `sb_memberrealname` TO `sb_agenthub_member_kyc`;
