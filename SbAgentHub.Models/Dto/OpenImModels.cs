using System.Text.Json.Serialization;

namespace SbAgentHub.Models.Dto
{
    /// <summary>
    /// OpenIM 通用响应
    /// </summary>
    public class OpenImResponse<T>
    {
        [JsonPropertyName("errCode")]
        public int ErrCode { get; set; }

        [JsonPropertyName("errMsg")]
        public string ErrMsg { get; set; }

        [JsonPropertyName("errDlt")]
        public string ErrDlt { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    /// <summary>
    /// OpenIM 用户注册请求
    /// </summary>
    public class OpenImRegisterRequest
    {
        [JsonPropertyName("users")]
        public List<OpenImUser> Users { get; set; }
    }

    /// <summary>
    /// OpenIM 用户信息
    /// </summary>
    public class OpenImUser
    {
        [JsonPropertyName("userID")]
        public string UserID { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("faceURL")]
        public string FaceURL { get; set; }
    }

    /// <summary>
    /// OpenIM 获取 Token 请求
    /// </summary>
    public class OpenImGetTokenRequest
    {
        [JsonPropertyName("userID")]
        public string UserID { get; set; }

        [JsonPropertyName("platformID")]
        public int PlatformID { get; set; } = 5; // Web
    }

    /// <summary>
    /// OpenIM Token 响应数据
    /// </summary>
    public class OpenImTokenData
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("expireTimeSeconds")]
        public long ExpireTimeSeconds { get; set; }
    }

    /// <summary>
    /// OpenIM 用户在线状态项
    /// </summary>
    public class OpenImOnlineStatusItem
    {
        [JsonPropertyName("userID")]
        public string UserID { get; set; }

        /// <summary>
        /// 1=online, 其他=offline
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    /// <summary>
    /// OpenIM 发送消息请求
    /// </summary>
    public class OpenImSendMsgRequest
    {
        [JsonPropertyName("sendID")]
        public string SendID { get; set; }

        [JsonPropertyName("recvID")]
        public string RecvID { get; set; }

        [JsonPropertyName("senderNickname")]
        public string SenderNickname { get; set; }

        [JsonPropertyName("senderFaceURL")]
        public string SenderFaceURL { get; set; }

        [JsonPropertyName("content")]
        public OpenImMsgContent Content { get; set; }

        [JsonPropertyName("contentType")]
        public int ContentType { get; set; } = 101; // Text
    }

    /// <summary>
    /// OpenIM 消息内容
    /// </summary>
    public class OpenImMsgContent
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
