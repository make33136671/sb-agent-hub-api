-- Migration: Rename OpenImUserId column to RealtimeUserId
-- 彻底去 OpenIM 命名 — DB 列名对齐 Realtime 语义
-- Run this BEFORE starting the updated application (after code rename, before AutoSyncStructure)

-- 1. Rename column OpenImUserId → RealtimeUserId on sb_agenthub_member
ALTER TABLE `sb_agenthub_member`
    CHANGE COLUMN `OpenImUserId` `RealtimeUserId` VARCHAR(100) NULL;

-- 2. (Optional) Update any seed/reference data that still uses the old column name
--    — none in current schema, placeholder for future reference
