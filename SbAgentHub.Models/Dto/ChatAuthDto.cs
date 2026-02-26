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
}
