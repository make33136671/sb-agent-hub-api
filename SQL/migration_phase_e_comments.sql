-- ============================================================
-- Migration: Phase E — Add COMMENT to all tables and columns
-- 每个表、每个字段都必须有说明注释
-- ============================================================

-- sb_agenthub_admin
ALTER TABLE `sb_agenthub_admin` COMMENT '系统管理员';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `ParentId` INT NOT NULL DEFAULT 0 COMMENT '父级Id';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `Username` VARCHAR(50) NOT NULL COMMENT '用户名';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `Password` VARCHAR(200) NOT NULL COMMENT '密码';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `RoleId` INT COMMENT '角色Id';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `Name` VARCHAR(255) COMMENT '名称';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `Avatar` VARCHAR(500) COMMENT '头像';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `Status` VARCHAR(20) COMMENT '管理员状态';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `LastOnlineTime` TIMESTAMP NULL COMMENT '最后登录时间';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_admin` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_admin_role
ALTER TABLE `sb_agenthub_admin_role` COMMENT '管理员角色';
ALTER TABLE `sb_agenthub_admin_role` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_admin_role` MODIFY COLUMN `Name` VARCHAR(100) COMMENT '角色名称';
ALTER TABLE `sb_agenthub_admin_role` MODIFY COLUMN `RoleKey` VARCHAR(50) COMMENT '角色标识';
ALTER TABLE `sb_agenthub_admin_role` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_admin_role` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_admin_role` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_admin_role_menu
ALTER TABLE `sb_agenthub_admin_role_menu` COMMENT '角色-菜单关联表';
ALTER TABLE `sb_agenthub_admin_role_menu` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_admin_role_menu` MODIFY COLUMN `RoleId` INT COMMENT '角色Id';
ALTER TABLE `sb_agenthub_admin_role_menu` MODIFY COLUMN `MenuId` INT COMMENT '菜单Id';

-- sb_agenthub_auto_reply
ALTER TABLE `sb_agenthub_auto_reply` COMMENT '自动回复规则';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `PlatformId` INT COMMENT '所属平台Id';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `TriggerType` VARCHAR(20) COMMENT '触发类型';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `Keywords` VARCHAR(500) COMMENT '关键词(逗号分隔)';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `MatchType` VARCHAR(20) COMMENT '匹配类型';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `ReplyContent` TEXT COMMENT '回复内容';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `ReplyType` VARCHAR(20) COMMENT '回复类型';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `Priority` INT COMMENT '优先级(越大越优先)';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `UpdateTime` TIMESTAMP NULL COMMENT '更新时间';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_auto_reply` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_chat_platform
ALTER TABLE `sb_agenthub_chat_platform` COMMENT '接入平台';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `PlatformName` VARCHAR(100) COMMENT '平台名称';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `Logo` VARCHAR(500) COMMENT '平台Logo URL';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `PlatformType` VARCHAR(20) COMMENT '平台类型';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `AccessType` VARCHAR(20) COMMENT '接入方式';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `PlatformKey` VARCHAR(64) NOT NULL COMMENT '平台密钥';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `Domain` VARCHAR(200) COMMENT '平台域名(兼容旧字段)';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `AllowedDomains` TEXT COMMENT '允许的域名列表(JSON数组)';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `WelcomeMessage` VARCHAR(500) COMMENT '欢迎语';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `IsAdminTakeover` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否开启管理员接管';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `TakeoverAdminId` INT NULL COMMENT '接管管理员Id';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_chat_platform` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_chat_user
ALTER TABLE `sb_agenthub_chat_user` COMMENT '聊天用户（Widget端用户）';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `ChatUserId` VARCHAR(128) NOT NULL COMMENT '聊天用户唯一标识（Realtime用户ID）';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `UserType` VARCHAR(20) COMMENT '用户类型';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `PlatformId` INT COMMENT '所属平台Id';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `ExternalUserId` VARCHAR(100) COMMENT '外部平台用户ID';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `Username` VARCHAR(100) COMMENT '用户名';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `Nickname` VARCHAR(100) COMMENT '昵称';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `Avatar` VARCHAR(500) COMMENT '头像';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `AssignedAgentId` INT NULL COMMENT '分配的客服Id';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `GroupId` VARCHAR(128) COMMENT '会话群组ID';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `ClientId` VARCHAR(64) COMMENT '客户端唯一标识';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `DeviceType` VARCHAR(50) COMMENT '设备类型';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `UserAgent` VARCHAR(500) COMMENT 'User-Agent';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `LastLoginIp` VARCHAR(50) COMMENT '最后登录IP';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `IpLocation` VARCHAR(200) COMMENT 'IP归属地';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `Status` VARCHAR(20) COMMENT '用户状态';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `OnlineStatus` VARCHAR(20) COMMENT '在线状态';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `NicknameRemark` VARCHAR(100) COMMENT '昵称备注（客服可见）';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `Remark` TEXT COMMENT '备注信息';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `FirstVisitTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '首次访问时间';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `LastActiveTime` TIMESTAMP NULL COMMENT '最后活跃时间';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_chat_user` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_chat_user_login_record
ALTER TABLE `sb_agenthub_chat_user_login_record` COMMENT '聊天用户登录记录';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `ChatUserDbId` INT COMMENT '对应的ChatUser.Id';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `PlatformId` INT COMMENT '平台Id';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `ClientId` VARCHAR(128) COMMENT '客户端唯一标识';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `IpAddress` VARCHAR(50) COMMENT 'IP地址';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `UserAgent` VARCHAR(500) COMMENT 'User-Agent';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `DeviceType` VARCHAR(50) COMMENT '设备类型';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `LoginTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '登录时间';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_chat_user_login_record` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_client_error_log
ALTER TABLE `sb_agenthub_client_error_log` COMMENT '客户端错误日志';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `Source` VARCHAR(50) COMMENT '错误来源';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `ErrorType` VARCHAR(100) COMMENT '错误类型';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `ErrorMessage` VARCHAR(2000) COMMENT '错误消息';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `Stack` TEXT COMMENT '堆栈信息';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `Url` VARCHAR(500) COMMENT '请求URL';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `UserAgent` VARCHAR(500) COMMENT 'User-Agent';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `UserId` VARCHAR(128) COMMENT '用户Id';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_client_error_log` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_customer_service_config
ALTER TABLE `sb_agenthub_customer_service_config` COMMENT '客服分配配置';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `AllocationRule` VARCHAR(20) COMMENT '分配规则';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `OfflineHandling` VARCHAR(20) COMMENT '离线处理方式';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `OfflineAutoReply` VARCHAR(500) COMMENT '离线自动回复内容';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `WorkTimeStart` VARCHAR(10) COMMENT '工作时间开始';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `WorkTimeEnd` VARCHAR(10) COMMENT '工作时间结束';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `EnableWorkTime` TINYINT(1) DEFAULT 0 COMMENT '是否启用工作时间限制';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `NonWorkTimeMessage` VARCHAR(500) COMMENT '非工作时间提示语';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `EnableBroadcast` TINYINT(1) DEFAULT 0 COMMENT '是否启用广播接待模式';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `EnableQueue` TINYINT(1) DEFAULT 0 COMMENT '是否启用排队功能';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `MaxConcurrentSessions` INT DEFAULT 50 COMMENT '最大并发会话数';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `UpdateTime` TIMESTAMP NULL COMMENT '更新时间';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_customer_service_config` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_file_record
ALTER TABLE `sb_agenthub_file_record` COMMENT '文件上传记录';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `PlatformId` INT COMMENT '所属平台Id';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `UploaderId` VARCHAR(64) COMMENT '上传者Id';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `UploaderType` VARCHAR(10) DEFAULT 'visitor' COMMENT '上传者类型: visitor/agent';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `FileName` VARCHAR(200) COMMENT '原始文件名';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `StoredPath` VARCHAR(500) COMMENT '存储路径';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `FileSize` BIGINT COMMENT '文件大小(字节)';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `FileType` VARCHAR(20) COMMENT '文件类型';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `MimeType` VARCHAR(100) COMMENT 'MIME类型';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_file_record` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_login_log
ALTER TABLE `sb_agenthub_login_log` COMMENT '登录日志';
ALTER TABLE `sb_agenthub_login_log` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_login_log` MODIFY COLUMN `UserId` INT COMMENT '用户Id（Member.Id或Admin.Id）';
ALTER TABLE `sb_agenthub_login_log` MODIFY COLUMN `UserType` INT COMMENT '用户类型: 0=H5会员 1=Admin管理员';
ALTER TABLE `sb_agenthub_login_log` MODIFY COLUMN `Username` VARCHAR(100) COMMENT '用户名';
ALTER TABLE `sb_agenthub_login_log` MODIFY COLUMN `IpAddress` VARCHAR(50) COMMENT 'IP地址';
ALTER TABLE `sb_agenthub_login_log` MODIFY COLUMN `UserAgent` VARCHAR(500) COMMENT 'User-Agent';
ALTER TABLE `sb_agenthub_login_log` MODIFY COLUMN `Status` INT COMMENT '状态: 0=成功 1=失败';
ALTER TABLE `sb_agenthub_login_log` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';

-- sb_agenthub_member
ALTER TABLE `sb_agenthub_member` COMMENT '业务用户（站长/客服）';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `Username` VARCHAR(255) COMMENT '用户名';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `Password` VARCHAR(255) COMMENT '密码';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `Avatar` VARCHAR(500) COMMENT '头像';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `Name` VARCHAR(255) COMMENT '名称';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `AgentNo` VARCHAR(50) COMMENT '工号';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `Role` VARCHAR(20) COMMENT '角色: Owner/Agent';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `OwnerId` INT NULL COMMENT '所属站长Id（Agent才有值）';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `RealtimeUserId` VARCHAR(100) COMMENT 'Realtime用户Id';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `OnlineStatus` VARCHAR(20) COMMENT '在线状态';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `MaxConcurrent` INT DEFAULT 10 COMMENT '最大同时服务数';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `CurrentCount` INT DEFAULT 0 COMMENT '当前服务数';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `WelcomeMessage` VARCHAR(500) COMMENT '欢迎语';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `Status` VARCHAR(20) COMMENT '账户状态';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `LastOnlineTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '最后在线时间';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_member` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_member_quota
ALTER TABLE `sb_agenthub_member_quota` COMMENT '站长配额分配';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `MemberId` INT COMMENT '站长Id';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `QuotaKey` VARCHAR(50) NOT NULL COMMENT '配额标识';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `QuotaValue` INT DEFAULT 0 COMMENT '配额值';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `GrantedBy` INT NULL COMMENT '授权管理员Id';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `GrantedByType` VARCHAR(10) DEFAULT 'Admin' COMMENT '授权者类型';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `ExpiresAt` TIMESTAMP NULL COMMENT '过期时间（NULL=永不过期）';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `Remark` VARCHAR(200) DEFAULT '' COMMENT '备注';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `UpdateTime` TIMESTAMP NULL COMMENT '更新时间';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_member_quota` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_order
ALTER TABLE `sb_agenthub_order` COMMENT '订单';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `OrderNo` VARCHAR(50) COMMENT '订单号';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `MemberId` INT COMMENT '站长Id';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `PackageId` INT COMMENT '套餐Id';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `PackageName` VARCHAR(100) COMMENT '套餐名称（冗余）';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `OrderType` VARCHAR(20) COMMENT '订单类型';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `Amount` DECIMAL(10,2) COMMENT '订单金额';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `Currency` VARCHAR(10) DEFAULT 'CNY' COMMENT '货币';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `Status` VARCHAR(20) COMMENT '订单状态';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `PaymentMethod` VARCHAR(20) COMMENT '支付方式';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `PaymentTime` TIMESTAMP NULL COMMENT '支付时间';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `PaymentTransactionId` VARCHAR(100) COMMENT '第三方支付交易号';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `EffectiveStartTime` TIMESTAMP NULL COMMENT '生效开始时间';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `EffectiveEndTime` TIMESTAMP NULL COMMENT '生效结束时间';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `Remark` VARCHAR(500) DEFAULT '' COMMENT '备注';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `UpdateTime` TIMESTAMP NULL COMMENT '更新时间';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_order` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_quota_usage
ALTER TABLE `sb_agenthub_quota_usage` COMMENT '配额实时用量';
ALTER TABLE `sb_agenthub_quota_usage` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_quota_usage` MODIFY COLUMN `MemberId` INT COMMENT '站长Id';
ALTER TABLE `sb_agenthub_quota_usage` MODIFY COLUMN `QuotaKey` VARCHAR(50) NOT NULL COMMENT '配额标识';
ALTER TABLE `sb_agenthub_quota_usage` MODIFY COLUMN `CurrentUsage` INT DEFAULT 0 COMMENT '当前使用量';
ALTER TABLE `sb_agenthub_quota_usage` MODIFY COLUMN `UpdateTime` TIMESTAMP NULL COMMENT '更新时间';
ALTER TABLE `sb_agenthub_quota_usage` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_quota_usage` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_quota_usage` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_satisfaction_rating
ALTER TABLE `sb_agenthub_satisfaction_rating` COMMENT '满意度评价';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `PlatformId` INT COMMENT '所属平台Id';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `ChatUserId` VARCHAR(64) COMMENT '聊天用户Id';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `AgentId` INT COMMENT '客服Id';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `Rating` INT COMMENT '评分1-5';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `Tags` VARCHAR(200) COMMENT '评价标签（逗号分隔）';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `Comment` VARCHAR(500) COMMENT '评价内容';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `ConversationId` VARCHAR(128) COMMENT '会话Id';
ALTER TABLE `sb_agenthub_satisfaction_rating` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';

-- sb_agenthub_user_preferences
ALTER TABLE `sb_agenthub_user_preferences` COMMENT '用户偏好设置';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `UserId` INT COMMENT '用户Id';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `UserType` VARCHAR(20) COMMENT '用户类型: Admin/Member';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `SoundEnabled` TINYINT(1) DEFAULT 1 COMMENT '新消息提示音开关';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `SoundFile` VARCHAR(100) COMMENT '提示音文件';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `SoundVolume` INT DEFAULT 80 COMMENT '提示音音量(0-100)';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `SystemNotificationEnabled` TINYINT(1) DEFAULT 1 COMMENT '系统通知提示音开关';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `SystemNotificationSound` VARCHAR(100) COMMENT '系统通知提示音文件';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `SystemNotificationVolume` INT DEFAULT 80 COMMENT '系统通知音量(0-100)';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `DesktopNotification` TINYINT(1) DEFAULT 1 COMMENT '桌面通知';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `AutoReply` TINYINT(1) DEFAULT 0 COMMENT '自动回复';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `AutoReplyMessage` VARCHAR(500) COMMENT '自动回复消息';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_user_preferences` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_visitor_form
ALTER TABLE `sb_agenthub_visitor_form` COMMENT '访客信息收集表单配置';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `PlatformId` INT COMMENT '所属平台Id';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `FormName` VARCHAR(100) DEFAULT '访客信息' COMMENT '表单名称';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `FormFields` TEXT COMMENT '表单字段配置(JSON)';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `IsRequired` TINYINT(1) DEFAULT 1 COMMENT '是否必填';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `UpdateTime` TIMESTAMP NULL COMMENT '更新时间';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_visitor_form` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_visitor_form_submission
ALTER TABLE `sb_agenthub_visitor_form_submission` COMMENT '访客表单提交记录';
ALTER TABLE `sb_agenthub_visitor_form_submission` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_visitor_form_submission` MODIFY COLUMN `FormId` INT COMMENT '表单Id';
ALTER TABLE `sb_agenthub_visitor_form_submission` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_visitor_form_submission` MODIFY COLUMN `PlatformId` INT COMMENT '所属平台Id';
ALTER TABLE `sb_agenthub_visitor_form_submission` MODIFY COLUMN `ChatUserId` VARCHAR(64) COMMENT '聊天用户Id';
ALTER TABLE `sb_agenthub_visitor_form_submission` MODIFY COLUMN `FormData` TEXT COMMENT '提交数据(JSON)';
ALTER TABLE `sb_agenthub_visitor_form_submission` MODIFY COLUMN `IpAddress` VARCHAR(50) COMMENT 'IP地址';
ALTER TABLE `sb_agenthub_visitor_form_submission` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';

-- sb_agenthub_visitor_queue
ALTER TABLE `sb_agenthub_visitor_queue` COMMENT '访客排队记录';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `ChatUserId` VARCHAR(64) COMMENT 'Realtime聊天用户Id';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `PlatformId` INT COMMENT '平台Id';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `OwnerId` INT COMMENT '站长Id';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `QueuePosition` INT COMMENT '排队位置';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `EnqueueTime` TIMESTAMP COMMENT '入队时间';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `Status` VARCHAR(20) COMMENT '状态: Waiting/Assigned/Cancelled/Timeout';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `AssignedAgentId` INT NULL COMMENT '分配的客服Id';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `AssignedTime` TIMESTAMP NULL COMMENT '分配时间';
ALTER TABLE `sb_agenthub_visitor_queue` MODIFY COLUMN `CancelTime` TIMESTAMP NULL COMMENT '取消时间';

-- sb_agenthub_access_violation_log
ALTER TABLE `sb_agenthub_access_violation_log` COMMENT '非法接入记录';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `PlatformId` INT COMMENT '平台Id';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `PlatformKey` VARCHAR(64) NOT NULL COMMENT '使用的平台Key';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `RequestDomain` VARCHAR(200) COMMENT '请求来源域名';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `RequestSource` VARCHAR(20) NOT NULL COMMENT '请求来源类型: js/vue/chat';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `ExpectedSource` VARCHAR(20) NOT NULL COMMENT '期望的接入类型';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `Reason` VARCHAR(50) NOT NULL COMMENT '拒绝原因';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `IpAddress` VARCHAR(50) COMMENT 'IP地址';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `UserAgent` VARCHAR(500) COMMENT 'User-Agent';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `RequestUrl` VARCHAR(500) COMMENT '请求的完整URL';
ALTER TABLE `sb_agenthub_access_violation_log` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';

-- sb_agenthub_migration_version
ALTER TABLE `sb_agenthub_migration_version` COMMENT '数据库迁移版本记录';
ALTER TABLE `sb_agenthub_migration_version` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_migration_version` MODIFY COLUMN `Version` VARCHAR(50) NOT NULL COMMENT '迁移版本号';
ALTER TABLE `sb_agenthub_migration_version` MODIFY COLUMN `Description` VARCHAR(200) COMMENT '迁移描述';
ALTER TABLE `sb_agenthub_migration_version` MODIFY COLUMN `AppliedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '执行时间';

-- sb_agenthub_menu
ALTER TABLE `sb_agenthub_menu` COMMENT '后台菜单';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `ParentId` INT COMMENT '父级Id';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `Name` VARCHAR(100) NOT NULL COMMENT '名称';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `Path` VARCHAR(200) NOT NULL COMMENT '路由路径';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `Component` VARCHAR(255) COMMENT '组件路径';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `Redirect` VARCHAR(255) COMMENT '重定向路由';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `Title` VARCHAR(100) NOT NULL COMMENT '显示标题';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `Icon` VARCHAR(255) COMMENT '图标';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `OrderNo` INT COMMENT '排序编号';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `HideInMenu` TINYINT(1) COMMENT '在菜单中隐藏';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `IgnoreKeepAlive` TINYINT(1) COMMENT '忽略保持存活';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_menu` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_pending_domain
ALTER TABLE `sb_agenthub_pending_domain` COMMENT '待审核域名';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `PlatformId` INT COMMENT '平台Id';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `OwnerId` INT COMMENT '所属站长Id';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `Domain` VARCHAR(200) NOT NULL COMMENT '请求来源域名';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `RequestCount` INT DEFAULT 1 COMMENT '请求次数';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `LastIpAddress` VARCHAR(50) COMMENT '最近一次请求IP';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `LastUserAgent` VARCHAR(500) COMMENT '最近一次User-Agent';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `Status` VARCHAR(20) COMMENT '状态: Pending/Approved/Rejected';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `FirstSeenTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '首次出现时间';
ALTER TABLE `sb_agenthub_pending_domain` MODIFY COLUMN `LastSeenTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '最后出现时间';

-- sb_agenthub_quota_config
ALTER TABLE `sb_agenthub_quota_config` COMMENT '配额定义';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `QuotaKey` VARCHAR(50) NOT NULL COMMENT '配额标识';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `QuotaName` VARCHAR(100) NOT NULL COMMENT '显示名称';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `QuotaType` INT DEFAULT 0 COMMENT '配额类型: 0=数量型 1=开关型 2=存储型';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `DefaultValue` INT DEFAULT 0 COMMENT '默认值';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `MinValue` INT DEFAULT 0 COMMENT '最小值';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `MaxValue` INT DEFAULT 999999 COMMENT '最大值';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `Unit` VARCHAR(20) COMMENT '单位描述';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `Description` VARCHAR(500) COMMENT '配额说明';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `SortOrder` INT DEFAULT 0 COMMENT '排序';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_quota_config` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_quota_package
ALTER TABLE `sb_agenthub_quota_package` COMMENT '套餐模板';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `PackageName` VARCHAR(100) COMMENT '套餐名称';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `PackageKey` VARCHAR(50) COMMENT '套餐标识';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `Description` VARCHAR(500) COMMENT '套餐描述';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `Price` DECIMAL(10,2) COMMENT '价格';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `PricePeriod` VARCHAR(20) DEFAULT 'month' COMMENT '计费周期: month/year';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `QuotaSettings` TEXT COMMENT '配额设置(JSON)';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `SortOrder` INT COMMENT '排序';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `CreateTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `UpdateTime` TIMESTAMP NULL COMMENT '更新时间';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '软删除标记';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `DeletedAt` TIMESTAMP NULL COMMENT '删除时间';
ALTER TABLE `sb_agenthub_quota_package` MODIFY COLUMN `DeletedBy` INT NULL COMMENT '删除人Id';

-- sb_agenthub_system_setting
ALTER TABLE `sb_agenthub_system_setting` COMMENT '系统设置';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `CategoryId` INT COMMENT '系统设置分类Id';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `Key` VARCHAR(100) NOT NULL COMMENT '设置系统键';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `Name` VARCHAR(100) NOT NULL COMMENT '设置名称';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `DisplayType` VARCHAR(20) COMMENT '设置界面显示类型';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `CantDelete` TINYINT(1) COMMENT '系统设置不能删除';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `DataContent` TEXT COMMENT '设置数据内容';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `DataContentConfig` TEXT COMMENT '设置数据选项';
ALTER TABLE `sb_agenthub_system_setting` MODIFY COLUMN `Sort` INT COMMENT '显示排序';

-- sb_agenthub_system_setting_category
ALTER TABLE `sb_agenthub_system_setting_category` COMMENT '系统设置分类';
ALTER TABLE `sb_agenthub_system_setting_category` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_system_setting_category` MODIFY COLUMN `Name` VARCHAR(50) NOT NULL COMMENT '设置分类名称';
ALTER TABLE `sb_agenthub_system_setting_category` MODIFY COLUMN `Icon` VARCHAR(255) COMMENT '图标';
ALTER TABLE `sb_agenthub_system_setting_category` MODIFY COLUMN `Sort` INT COMMENT '显示排序';

-- ============================================================
-- Historical tables (not managed by FreeSql entities)
-- ============================================================

-- SYS_BackgroundTaskLog
ALTER TABLE `SYS_BackgroundTaskLog` COMMENT '后台任务日志';
ALTER TABLE `SYS_BackgroundTaskLog` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `SYS_BackgroundTaskLog` MODIFY COLUMN `Source` INT NOT NULL DEFAULT 0 COMMENT '任务来源';
ALTER TABLE `SYS_BackgroundTaskLog` MODIFY COLUMN `TaskName` VARCHAR(200) NOT NULL COMMENT '任务名称';
ALTER TABLE `SYS_BackgroundTaskLog` MODIFY COLUMN `LogLevel` INT NOT NULL DEFAULT 2 COMMENT '日志级别: 0=Trace 1=Debug 2=Info 3=Warn 4=Error 5=Fatal';
ALTER TABLE `SYS_BackgroundTaskLog` MODIFY COLUMN `Message` VARCHAR(2000) NOT NULL COMMENT '日志消息';
ALTER TABLE `SYS_BackgroundTaskLog` MODIFY COLUMN `Exception` TEXT NULL COMMENT '异常信息';
ALTER TABLE `SYS_BackgroundTaskLog` MODIFY COLUMN `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `SYS_BackgroundTaskLog` MODIFY COLUMN `ExecutionRecordId` VARCHAR(50) NULL COMMENT '执行记录Id';

-- sb_agenthub_email_config
ALTER TABLE `sb_agenthub_email_config` COMMENT '邮件服务配置';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `SmtpServer` VARCHAR(255) NULL COMMENT 'SMTP服务器地址';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `SmtpPort` INT NOT NULL DEFAULT 587 COMMENT 'SMTP端口';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `Username` VARCHAR(255) NULL COMMENT 'SMTP用户名';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `Password` VARCHAR(255) NULL COMMENT 'SMTP密码';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `FromEmail` VARCHAR(255) NULL COMMENT '发件人邮箱';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `FromName` VARCHAR(255) NULL COMMENT '发件人名称';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `EnableSsl` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用SSL';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `IsActive` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否激活';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_email_config` MODIFY COLUMN `UpdatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间';

-- sb_agenthub_email_template
ALTER TABLE `sb_agenthub_email_template` COMMENT '邮件模板';
ALTER TABLE `sb_agenthub_email_template` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_email_template` MODIFY COLUMN `Code` VARCHAR(100) NULL COMMENT '模板编码';
ALTER TABLE `sb_agenthub_email_template` MODIFY COLUMN `Name` VARCHAR(255) NULL COMMENT '模板名称';
ALTER TABLE `sb_agenthub_email_template` MODIFY COLUMN `Subject` VARCHAR(500) NULL COMMENT '邮件主题';
ALTER TABLE `sb_agenthub_email_template` MODIFY COLUMN `Body` TEXT NULL COMMENT '邮件正文（支持HTML）';
ALTER TABLE `sb_agenthub_email_template` MODIFY COLUMN `IsActive` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用';
ALTER TABLE `sb_agenthub_email_template` MODIFY COLUMN `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';
ALTER TABLE `sb_agenthub_email_template` MODIFY COLUMN `UpdatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间';

-- sb_agenthub_email_verification_code
ALTER TABLE `sb_agenthub_email_verification_code` COMMENT '邮箱验证码';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `Email` VARCHAR(255) NULL COMMENT '邮箱地址';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `Code` VARCHAR(10) NULL COMMENT '验证码';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `Type` INT NOT NULL DEFAULT 0 COMMENT '验证码类型: 0=注册 1=找回密码 2=修改邮箱';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `VerifyCount` INT NOT NULL DEFAULT 0 COMMENT '验证尝试次数';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `IsUsed` TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否已使用';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `VerifiedAt` TIMESTAMP NULL COMMENT '验证时间';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `IpAddress` VARCHAR(50) NULL COMMENT 'IP地址';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `ExpireAt` TIMESTAMP NOT NULL COMMENT '过期时间';
ALTER TABLE `sb_agenthub_email_verification_code` MODIFY COLUMN `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';

-- sb_agenthub_email_log
ALTER TABLE `sb_agenthub_email_log` COMMENT '邮件发送日志';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `TemplateId` INT NOT NULL DEFAULT 0 COMMENT '邮件模板Id';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `ToEmail` VARCHAR(255) NULL COMMENT '收件人邮箱';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `Subject` VARCHAR(500) NULL COMMENT '邮件主题';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `Body` TEXT NULL COMMENT '邮件正文';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `Status` VARCHAR(50) NULL COMMENT '发送状态: Pending/Sent/Failed';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `ErrorMessage` TEXT NULL COMMENT '错误信息';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `Provider` VARCHAR(50) NULL COMMENT '邮件服务提供商';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `SentAt` TIMESTAMP NULL COMMENT '发送时间';
ALTER TABLE `sb_agenthub_email_log` MODIFY COLUMN `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间';

-- sb_agenthub_member_kyc
ALTER TABLE `sb_agenthub_member_kyc` COMMENT '会员实名认证（KYC）';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `Id` INT AUTO_INCREMENT COMMENT '主键Id';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `MemberId` INT NOT NULL COMMENT '会员Id（Member.Id）';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `RealName` VARCHAR(100) NULL COMMENT '真实姓名';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `IdNumber` VARCHAR(100) NULL COMMENT '证件号码';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `IdFrontImage` VARCHAR(500) NULL COMMENT '证件正面照URL';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `IdBackImage` VARCHAR(500) NULL COMMENT '证件背面照URL';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `Status` VARCHAR(50) NOT NULL DEFAULT 'Reviewing' COMMENT '审核状态: Reviewing/Approved/Rejected';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `RejectReason` TEXT NULL COMMENT '拒绝原因';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `SubmitTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '提交时间';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `ReviewTime` TIMESTAMP NULL COMMENT '审核时间';
ALTER TABLE `sb_agenthub_member_kyc` MODIFY COLUMN `ReviewedBy` INT NULL COMMENT '审核人Id（Admin.Id）';

-- Record this migration
INSERT INTO `sb_agenthub_migration_version` (`Version`, `Description`) VALUES
    ('20260820_02', 'Phase E: Added COMMENT to all tables and columns (including historical tables)');
