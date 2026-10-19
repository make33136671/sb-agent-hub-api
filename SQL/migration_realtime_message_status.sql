-- Migration: Realtime 消息状态字段升级
-- 为 chat_rt_message 表新增消息状态相关字段（status / delivered_at / read_at / edited_at / previous_content）
-- 适配新版 Sb.Framework.Chat.Realtime 消息状态机（8 态：Sending/Sent/Delivered/Read/Failed/Withdrawn/Edited/Deleted）
-- Run this BEFORE starting the updated application (before CodeFirst SyncStructure)

-- 1. 新增 status 字段（消息状态，枚举名存储：Sending/Sent/Delivered/Read/Failed/Withdrawn/Edited/Deleted）
ALTER TABLE `chat_rt_message`
    ADD COLUMN `status` VARCHAR(16) NOT NULL DEFAULT 'Sent' COMMENT '消息状态' AFTER `seq`;

-- 2. 新增 delivered_at 字段（送达时间，UTC）
ALTER TABLE `chat_rt_message`
    ADD COLUMN `delivered_at` TIMESTAMP NULL COMMENT '送达时间' AFTER `status`;

-- 3. 新增 read_at 字段（已读时间，UTC）
ALTER TABLE `chat_rt_message`
    ADD COLUMN `read_at` TIMESTAMP NULL COMMENT '已读时间' AFTER `delivered_at`;

-- 4. 新增 edited_at 字段（编辑时间，UTC）
ALTER TABLE `chat_rt_message`
    ADD COLUMN `edited_at` TIMESTAMP NULL COMMENT '编辑时间' AFTER `read_at`;

-- 5. 新增 previous_content 字段（编辑前内容，审计用）
ALTER TABLE `chat_rt_message`
    ADD COLUMN `previous_content` LONGTEXT NULL COMMENT '编辑前内容' AFTER `edited_at`;

-- 6. 历史数据回填：IsWithdrawn=true 的消息同步设置 status='Withdrawn'
UPDATE `chat_rt_message`
SET `status` = 'Withdrawn'
WHERE `is_withdrawn` = TRUE;

-- 7. 新增 status 索引（快速筛选某状态消息）
CREATE INDEX `ix_rt_message_status` ON `chat_rt_message` (`status`);
