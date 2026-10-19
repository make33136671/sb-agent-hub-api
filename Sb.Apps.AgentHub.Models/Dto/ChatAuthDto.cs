namespace Sb.Apps.AgentHub.Models.Dto
{
    /// <summary>
    /// 访客认证请求
    /// </summary>
    public class VisitorAuthRequest
    {
        /// <summary>
        /// 平台密钥
        /// </summary>
        public required string PlatformKey { get; set; }
        /// <summary>
        /// 客户端唯一标识
        /// </summary>
        public required string ClientId { get; set; }
        /// <summary>
        /// 外部平台用户ID
        /// </summary>
        public required string ExternalUserId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public required string Username { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public required string Nickname { get; set; }
        /// <summary>
        /// 头像URL
        /// </summary>
        public required string Avatar { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public required string DeviceType { get; set; }
        /// <summary>
        /// User-Agent
        /// </summary>
        public required string UserAgent { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public required string IpAddress { get; set; }

        /// <summary>
        /// 请求来源类型: js/vue/chat
        /// </summary>
        public required string Source { get; set; }

        /// <summary>
        /// 请求来源域名（由后端从 Origin/Referer 提取，前端不传）
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string RequestDomain { get; set; } = null!;
    }

    /// <summary>
    /// 访客认证响应
    /// </summary>
    public class VisitorAuthResponse
    {
        /// <summary>
        /// 聊天用户ID
        /// </summary>
        public required string ChatUserId { get; set; }
        /// <summary>
        /// 访问令牌
        /// </summary>
        public required string Token { get; set; }
        /// <summary>
        /// 令牌过期时间戳
        /// </summary>
        public long TokenExpire { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public required string Nickname { get; set; }
        /// <summary>
        /// 头像URL
        /// </summary>
        public required string Avatar { get; set; }
        /// <summary>
        /// 客服ID
        /// </summary>
        public int? AgentId { get; set; }
        /// <summary>
        /// 客服名称
        /// </summary>
        public string AgentName { get; set; } = null!;
        /// <summary>
        /// 客服头像
        /// </summary>
        public string AgentAvatar { get; set; } = null!;
        /// <summary>
        /// 客服用户ID
        /// </summary>
        public string AgentUserId { get; set; } = null!;
        /// <summary>
        /// 客服是否在线
        /// </summary>
        public bool AgentOnline { get; set; }
        /// <summary>
        /// 离线消息
        /// </summary>
        public string OfflineMessage { get; set; } = null!;
        /// <summary>
        /// 欢迎消息
        /// </summary>
        public string WelcomeMessage { get; set; } = null!;
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = null!;

        /// <summary>会话群组 ID</summary>
        public string GroupId { get; set; } = null!;

        /// <summary>是否在排队中</summary>
        public bool IsQueued { get; set; }
        /// <summary>排队位置</summary>
        public int QueuePosition { get; set; }
        /// <summary>总排队人数</summary>
        public int QueueWaitingCount { get; set; }
        /// <summary>预计等待分钟数</summary>
        public int EstimatedWaitMinutes { get; set; }
    }

    /// <summary>
    /// 刷新 Token 请求
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// 聊天用户ID
        /// </summary>
        public required string ChatUserId { get; set; }
        /// <summary>
        /// 平台密钥
        /// </summary>
        public required string PlatformKey { get; set; }
    }

    /// <summary>
    /// Token 响应
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public required string Token { get; set; }
        /// <summary>
        /// 过期时间（秒）
        /// </summary>
        public long ExpireTimeSeconds { get; set; }
    }

    /// <summary>
    /// SDK 端 GetToken 请求（FormData）
    /// </summary>
    public class GetTokenFormRequest
    {
        /// <summary>
        /// 平台密钥
        /// </summary>
        public required string PlatformKey { get; set; }
        /// <summary>
        /// 客户端唯一标识
        /// </summary>
        public required string ClientId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public required string UserId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public required string Nickname { get; set; }
        /// <summary>
        /// 头像URL
        /// </summary>
        public required string Avatar { get; set; }
        /// <summary>
        /// 元数据
        /// </summary>
        public string Metadata { get; set; } = null!;
        /// <summary>
        /// 平台类型
        /// </summary>
        public int PlatformType { get; set; }
        /// <summary>
        /// 设备信息
        /// </summary>
        public string DeviceInfo { get; set; } = null!;
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; } = null!;

        /// <summary>
        /// 请求来源类型: js/vue/chat
        /// </summary>
        public required string Source { get; set; }
    }

    /// <summary>
    /// SDK 端 GetToken 响应（嵌套格式，匹配 realtime-core-js SDK）
    /// </summary>
    public class GetTokenSdkResponse
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public required string UserID { get; set; }
        /// <summary>
        /// 访问令牌
        /// </summary>
        public required string Token { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public long ExpireTime { get; set; }
        /// <summary>
        /// 用户资料
        /// </summary>
        public SdkUserProfile User { get; set; } = null!;
        /// <summary>
        /// 客服信息
        /// </summary>
        public SdkAgentInfo Agent { get; set; } = null!;
        /// <summary>
        /// 平台信息
        /// </summary>
        public SdkPlatformInfo Platform { get; set; } = null!;
        /// <summary>
        /// 功能开关
        /// </summary>
        public SdkFeatureFlags Features { get; set; } = null!;
        /// <summary>
        /// 排队信息
        /// </summary>
        public SdkQueueInfo Queue { get; set; } = null!;
        /// <summary>
        /// 访客表单配置
        /// </summary>
        public SdkVisitorFormConfig VisitorForm { get; set; } = null!;

        /// <summary>
        /// Realtime 连接配置（服务端下发）
        /// </summary>
        public SdkRealtimeConfig RealtimeConfig { get; set; } = null!;
    }

    /// <summary>
    /// SDK 排队信息
    /// </summary>
    public class SdkQueueInfo
    {
        /// <summary>
        /// 是否在排队中
        /// </summary>
        public bool IsQueued { get; set; }
        /// <summary>
        /// 排队位置
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// 总等待人数
        /// </summary>
        public int TotalWaiting { get; set; }
        /// <summary>
        /// 预计等待分钟数
        /// </summary>
        public int EstimatedWaitMinutes { get; set; }
    }

    /// <summary>
    /// SDK 用户资料
    /// </summary>
    public class SdkUserProfile
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public required string UserId { get; set; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }
        /// <summary>
        /// 是否新用户
        /// </summary>
        public bool IsNewUser { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public required string Nickname { get; set; }
        /// <summary>
        /// 头像URL
        /// </summary>
        public required string Avatar { get; set; }
        /// <summary>
        /// 外部用户ID
        /// </summary>
        public string ExternalUserId { get; set; } = null!;
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; } = null!;
    }

    /// <summary>
    /// SDK 客服信息
    /// </summary>
    public class SdkAgentInfo
    {
        /// <summary>
        /// 客服用户ID
        /// </summary>
        public required string AgentUserId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public required string Nickname { get; set; }
        /// <summary>
        /// 头像URL
        /// </summary>
        public required string Avatar { get; set; }
        /// <summary>
        /// 在线状态
        /// </summary>
        public int OnlineStatus { get; set; }
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool IsOnline { get; set; }
        /// <summary>
        /// 欢迎消息
        /// </summary>
        public string WelcomeMessage { get; set; } = null!;
        /// <summary>
        /// 是否自动发送欢迎语
        /// </summary>
        public bool AutoSendWelcome { get; set; }
        /// <summary>
        /// 预计算的 Realtime 会话 ID，格式: si_{agentUserId}_{chatUserId}
        /// </summary>
        public string ConversationId { get; set; } = null!;

        /// <summary>
        /// 会话群组 ID（统一群聊模式）
        /// </summary>
        public string GroupId { get; set; } = null!;
    }

    /// <summary>
    /// SDK 平台信息
    /// </summary>
    public class SdkPlatformInfo
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformId { get; set; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public required string PlatformName { get; set; }
        /// <summary>
        /// 离线消息
        /// </summary>
        public string OfflineMessage { get; set; } = null!;
        /// <summary>
        /// 是否允许离线消息
        /// </summary>
        public bool AllowOfflineMessage { get; set; }
    }

    /// <summary>
    /// SDK 功能开关
    /// </summary>
    public class SdkFeatureFlags
    {
        /// <summary>
        /// 是否允许发送图片
        /// </summary>
        public bool AllowImage { get; set; } = true;
        /// <summary>
        /// 是否允许发送文件
        /// </summary>
        public bool AllowFile { get; set; } = true;
        /// <summary>
        /// 是否允许语音
        /// </summary>
        public bool AllowVoice { get; set; } = false;
        /// <summary>
        /// 是否显示在线状态
        /// </summary>
        public bool ShowOnlineStatus { get; set; } = true;
        /// <summary>
        /// 是否显示已读状态
        /// </summary>
        public bool ShowReadStatus { get; set; } = false;
        /// <summary>
        /// 是否启用满意度评价
        /// </summary>
        public bool EnableSatisfactionRating { get; set; } = false;
        /// <summary>
        /// 是否启用访客表单
        /// </summary>
        public bool EnableVisitorForm { get; set; } = false;
        /// <summary>
        /// 是否启用排队功能
        /// </summary>
        public bool EnableQueue { get; set; } = false;
        /// <summary>
        /// 是否启用广播
        /// </summary>
        public bool EnableBroadcast { get; set; } = false;
    }

    /// <summary>
    /// Realtime 连接配置（服务端下发，客户端无需自行配置）
    /// </summary>
    public class SdkRealtimeConfig
    {
        /// <summary>
        /// API 基础地址
        /// </summary>
        public required string ApiUrl { get; set; }

        /// <summary>
        /// Realtime Hub WebSocket 地址
        /// </summary>
        public required string WsUrl { get; set; }
    }

    /// <summary>
    /// SDK 访客表单配置
    /// </summary>
    public class SdkVisitorFormConfig
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 表单名称
        /// </summary>
        public required string FormName { get; set; }
        /// <summary>
        /// 表单字段配置（JSON）
        /// </summary>
        public string FormFields { get; set; } = null!;
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; }
    }

    /// <summary>
    /// 访客离线通知请求（由 sendBeacon 调用）
    /// </summary>
    public class VisitorOfflineRequest
    {
        /// <summary>
        /// 聊天用户ID
        /// </summary>
        public required string ChatUserId { get; set; }
        /// <summary>
        /// 客服用户ID
        /// </summary>
        public required string AgentUserId { get; set; }
        /// <summary>
        /// 离线原因
        /// </summary>
        public string Reason { get; set; } = null!;
        /// <summary>
        /// 会话时长（秒）
        /// </summary>
        public int? SessionDuration { get; set; }
    }

    /// <summary>
    /// 消息已读回执请求
    /// </summary>
    public class MessageReadRequest
    {
        /// <summary>
        /// 聊天用户ID
        /// </summary>
        public required string ChatUserId { get; set; }
        /// <summary>
        /// 客服用户ID
        /// </summary>
        public required string AgentUserId { get; set; }
        /// <summary>
        /// 会话ID
        /// </summary>
        public required string ConversationId { get; set; }
    }

    /// <summary>
    /// 刷新会话请求（容灾恢复）
    /// </summary>
    public class RefreshSessionRequest
    {
        /// <summary>
        /// 聊天用户ID
        /// </summary>
        public required string ChatUserId { get; set; }
        /// <summary>
        /// 客户端唯一标识
        /// </summary>
        public required string ClientId { get; set; }
        /// <summary>
        /// 平台密钥
        /// </summary>
        public required string PlatformKey { get; set; }
    }

    /// <summary>
    /// 取消排队请求
    /// </summary>
    public class CancelQueueRequest
    {
        /// <summary>
        /// 聊天用户ID
        /// </summary>
        public required string ChatUserId { get; set; }
    }

    /// <summary>
    /// 访客表单提交请求
    /// </summary>
    public class SubmitVisitorFormRequest
    {
        /// <summary>
        /// 表单ID
        /// </summary>
        public int FormId { get; set; }
        /// <summary>
        /// 聊天用户ID
        /// </summary>
        public required string ChatUserId { get; set; }
        /// <summary>
        /// 表单数据（JSON）
        /// </summary>
        public required string FormData { get; set; }
    }

    /// <summary>
    /// 刷新会话响应
    /// </summary>
    public class RefreshSessionResponse
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public required string Token { get; set; }
        /// <summary>
        /// 过期时间（秒）
        /// </summary>
        public long ExpireTimeSeconds { get; set; }
        /// <summary>
        /// 客服用户ID
        /// </summary>
        public string AgentUserId { get; set; } = null!;
        /// <summary>
        /// 客服是否在线
        /// </summary>
        public bool AgentOnline { get; set; }
    }
}
