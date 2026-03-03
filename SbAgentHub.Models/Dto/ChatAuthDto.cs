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
    }
}
