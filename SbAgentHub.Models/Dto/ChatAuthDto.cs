namespace SbAgentHub.Models.Dto
{
    /// <summary>
    /// 访客认证请求
    /// </summary>
    public class VisitorAuthRequest
    {
        public string PlatformKey { get; set; }
        public string ClientId { get; set; }
        public string ExternalUserId { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public string DeviceType { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }

        /// <summary>
        /// 请求来源类型: js/vue/chat
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 请求来源域名（由后端从 Origin/Referer 提取，前端不传）
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string RequestDomain { get; set; }
    }

    /// <summary>
    /// 访客认证响应
    /// </summary>
    public class VisitorAuthResponse
    {
        public string ChatUserId { get; set; }
        public string ImToken { get; set; }
        public long ImTokenExpire { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public int? AgentId { get; set; }
        public string AgentName { get; set; }
        public string AgentAvatar { get; set; }
        public string AgentImUserId { get; set; }
        public bool AgentOnline { get; set; }
        public string OfflineMessage { get; set; }
        public string WelcomeMessage { get; set; }
        public string PlatformName { get; set; }

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
    /// 刷新 IM Token 请求
    /// </summary>
    public class RefreshImTokenRequest
    {
        public string ChatUserId { get; set; }
        public string PlatformKey { get; set; }
    }

    /// <summary>
    /// IM Token 响应
    /// </summary>
    public class ImTokenResponse
    {
        public string Token { get; set; }
        public long ExpireTimeSeconds { get; set; }
    }

    /// <summary>
    /// SDK 端 GetImToken 请求（FormData）
    /// </summary>
    public class GetImTokenFormRequest
    {
        public string PlatformKey { get; set; }
        public string ClientId { get; set; }
        public string UserId { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public string Metadata { get; set; }
        public int PlatformType { get; set; }
        public string DeviceInfo { get; set; }
        public string DeviceId { get; set; }

        /// <summary>
        /// 请求来源类型: js/vue/chat
        /// </summary>
        public string Source { get; set; }
    }

    /// <summary>
    /// SDK 端 GetImToken 响应（嵌套格式，匹配 openim-core-js SDK）
    /// </summary>
    public class GetImTokenSdkResponse
    {
        public string UserID { get; set; }
        public string Token { get; set; }
        public long ExpireTime { get; set; }
        public SdkUserProfile User { get; set; }
        public SdkAgentInfo Agent { get; set; }
        public SdkPlatformInfo Platform { get; set; }
        public SdkFeatureFlags Features { get; set; }
        public SdkQueueInfo Queue { get; set; }
        public SdkVisitorFormConfig VisitorForm { get; set; }

        /// <summary>
        /// IM 服务器连接配置（服务端下发）
        /// </summary>
        public SdkImConfig ImConfig { get; set; }
    }

    public class SdkQueueInfo
    {
        public bool IsQueued { get; set; }
        public int Position { get; set; }
        public int TotalWaiting { get; set; }
        public int EstimatedWaitMinutes { get; set; }
    }

    public class SdkUserProfile
    {
        public string UserId { get; set; }
        public int UserType { get; set; }
        public bool IsNewUser { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public string ExternalUserId { get; set; }
        public string DeviceId { get; set; }
    }

    public class SdkAgentInfo
    {
        public string AgentUserId { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public int OnlineStatus { get; set; }
        public bool IsOnline { get; set; }
        public string WelcomeMessage { get; set; }
        public bool AutoSendWelcome { get; set; }
        /// <summary>
        /// 预计算的 OpenIM 会话 ID，格式: si_{agentImUserId}_{chatUserId}
        /// </summary>
        public string ConversationId { get; set; }
    }

    public class SdkPlatformInfo
    {
        public int PlatformId { get; set; }
        public string PlatformName { get; set; }
        public string OfflineMessage { get; set; }
        public bool AllowOfflineMessage { get; set; }
    }

    public class SdkFeatureFlags
    {
        public bool AllowImage { get; set; } = true;
        public bool AllowFile { get; set; } = true;
        public bool AllowVoice { get; set; } = false;
        public bool ShowOnlineStatus { get; set; } = true;
        public bool ShowReadStatus { get; set; } = false;
        public bool EnableSatisfactionRating { get; set; } = false;
        public bool EnableVisitorForm { get; set; } = false;
        public bool EnableQueue { get; set; } = false;
    }

    /// <summary>
    /// IM 服务器连接配置（服务端下发，客户端无需自行配置）
    /// </summary>
    public class SdkImConfig
    {
        /// <summary>
        /// OpenIM HTTP API 地址
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// OpenIM WebSocket 地址
        /// </summary>
        public string WsUrl { get; set; }
    }

    public class SdkVisitorFormConfig
    {
        public int FormId { get; set; }
        public string FormName { get; set; }
        public string FormFields { get; set; }
        public bool IsRequired { get; set; }
    }

    /// <summary>
    /// 访客离线通知请求（由 sendBeacon 调用）
    /// </summary>
    public class VisitorOfflineRequest
    {
        public string ChatUserId { get; set; }
        public string AgentImUserId { get; set; }
        public string Reason { get; set; }
        public int? SessionDuration { get; set; }
    }

    /// <summary>
    /// 消息已读回执请求
    /// </summary>
    public class MessageReadRequest
    {
        public string ChatUserId { get; set; }
        public string AgentImUserId { get; set; }
        public string ConversationId { get; set; }
    }

    /// <summary>
    /// 刷新会话请求（容灾恢复）
    /// </summary>
    public class RefreshSessionRequest
    {
        public string ChatUserId { get; set; }
        public string ClientId { get; set; }
        public string PlatformKey { get; set; }
    }

    /// <summary>
    /// 取消排队请求
    /// </summary>
    public class CancelQueueRequest
    {
        public string ChatUserId { get; set; }
    }

    /// <summary>
    /// 访客表单提交请求
    /// </summary>
    public class SubmitVisitorFormRequest
    {
        public int FormId { get; set; }
        public string ChatUserId { get; set; }
        public string FormData { get; set; }
    }

    /// <summary>
    /// 刷新会话响应
    /// </summary>
    public class RefreshSessionResponse
    {
        public string Token { get; set; }
        public long ExpireTimeSeconds { get; set; }
        public string AgentImUserId { get; set; }
        public bool AgentOnline { get; set; }
    }
}
