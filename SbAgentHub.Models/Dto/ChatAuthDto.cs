namespace SbAgentHub.Models.Dto
{
    /// <summary>
    /// 访客认证请求
    /// </summary>
    public class VisitorAuthRequest
    {
        public string PlatformKey { get; set; }
        public string ClientId { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public string DeviceType { get; set; }
        public string UserAgent { get; set; }
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
        public string AgentImUserId { get; set; }
        public string WelcomeMessage { get; set; }
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
        public string ImToken { get; set; }
        public long ImTokenExpire { get; set; }
    }
}
