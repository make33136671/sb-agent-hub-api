-- Migration: Create Background Task Log Table

CREATE TABLE IF NOT EXISTS `SYS_BackgroundTaskLog` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Source` INT NOT NULL DEFAULT 0,
    `TaskName` VARCHAR(200) NOT NULL,
    `LogLevel` INT NOT NULL DEFAULT 2,
    `Message` VARCHAR(2000) NOT NULL,
    `Exception` TEXT NULL,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ExecutionRecordId` VARCHAR(50) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX `idx_task_log_name` ON `SYS_BackgroundTaskLog` (`TaskName`);
CREATE INDEX `idx_task_log_level` ON `SYS_BackgroundTaskLog` (`LogLevel`);
CREATE INDEX `idx_task_log_time` ON `SYS_BackgroundTaskLog` (`CreatedTime`);
