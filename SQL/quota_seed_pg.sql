-- ============================================================
-- 配额配置种子数据 (PostgreSQL)
-- 每次应用启动时由 Program.EnsureQuotaSeedData 自动检查并补全
-- 此文件供手动初始化或迁移参考
-- ============================================================

INSERT INTO sb_agenthub_quota_config
("QuotaKey","QuotaName","QuotaType","DefaultValue","MinValue","MaxValue","Unit","Description","SortOrder","IsEnabled","IsDeleted")
SELECT 'max_platforms','最大平台数',0,999999,0,999999,'个','站长可创建的平台数量上限',1,true,false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_quota_config WHERE "QuotaKey" = 'max_platforms');

INSERT INTO sb_agenthub_quota_config
("QuotaKey","QuotaName","QuotaType","DefaultValue","MinValue","MaxValue","Unit","Description","SortOrder","IsEnabled","IsDeleted")
SELECT 'max_agents','最大客服数',0,999999,0,999999,'个','站长可创建的客服账号上限',2,true,false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_quota_config WHERE "QuotaKey" = 'max_agents');

INSERT INTO sb_agenthub_quota_config
("QuotaKey","QuotaName","QuotaType","DefaultValue","MinValue","MaxValue","Unit","Description","SortOrder","IsEnabled","IsDeleted")
SELECT 'max_concurrent_sessions','最大并发会话',0,999999,0,999999,'个','同时在线的访客会话数上限',3,true,false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_quota_config WHERE "QuotaKey" = 'max_concurrent_sessions');

INSERT INTO sb_agenthub_quota_config
("QuotaKey","QuotaName","QuotaType","DefaultValue","MinValue","MaxValue","Unit","Description","SortOrder","IsEnabled","IsDeleted")
SELECT 'max_daily_visitors','每日访客数',0,999999,0,999999,'人次','每日新增访客咨询上限',4,true,false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_quota_config WHERE "QuotaKey" = 'max_daily_visitors');

INSERT INTO sb_agenthub_quota_config
("QuotaKey","QuotaName","QuotaType","DefaultValue","MinValue","MaxValue","Unit","Description","SortOrder","IsEnabled","IsDeleted")
SELECT 'storage_quota_mb','存储空间',2,999999,0,999999,'MB','文件存储配额(MB)',5,true,false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_quota_config WHERE "QuotaKey" = 'storage_quota_mb');

INSERT INTO sb_agenthub_quota_config
("QuotaKey","QuotaName","QuotaType","DefaultValue","MinValue","MaxValue","Unit","Description","SortOrder","IsEnabled","IsDeleted")
SELECT 'message_retention_days','消息保留天数',0,999999,0,999999,'天','聊天消息保留天数',6,true,false
WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_quota_config WHERE "QuotaKey" = 'message_retention_days');
