using Microsoft.Extensions.Logging;
using SbAgentHub.Core;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models.Dto;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SbAgentHub.Services
{
    /// <summary>
    /// OpenIM 会话服务 — 封装 OpenIM 会话相关的服务端 API
    /// </summary>
    [AutoDependencyInjection]
    public class OpenImConversationService
    {
        private readonly ILogger<OpenImConversationService> _logger;
        private readonly OpenImAuthService _authService;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public OpenImConversationService(ILogger<OpenImConversationService> logger, OpenImAuthService authService)
        {
            _logger = logger;
            _authService = authService;

            var config = AppGlobal.AppSettings?.OpenIM;
            _apiUrl = config?.ImApiUrl?.TrimEnd('/') ?? "";

            _httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(_apiUrl))
            {
                _httpClient.BaseAddress = new Uri(_apiUrl);
            }
        }

        /// <summary>
        /// 获取排序会话列表（包含最新消息摘要、未读数）
        /// </summary>
        public async Task<OpenImSortedConversationResult> GetSortedConversationsAsync(string userId, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(userId))
                return null;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/conversation/get_sorted_conversation_list");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    userID = userId,
                    conversationIDs = Array.Empty<string>(),
                    pagination = new
                    {
                        pageNumber = page,
                        showNumber = pageSize
                    }
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<OpenImSortedConversationResult>>();

                if (result?.ErrCode == 0)
                {
                    return result.Data;
                }

                _logger.LogWarning("获取排序会话列表失败: {ErrCode} {ErrMsg}", result?.ErrCode, result?.ErrMsg);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取排序会话列表异常: UserId={UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// 获取指定用户的会话列表
        /// </summary>
        public async Task<List<OpenImConversationInfo>> GetUserConversationsAsync(string userId, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(userId))
                return new List<OpenImConversationInfo>();

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/conversation/get_owner_conversation");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    userID = userId,
                    pagination = new
                    {
                        pageNumber = page,
                        showNumber = pageSize
                    }
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<OpenImOwnerConversationResult>>();

                if (result?.ErrCode == 0)
                {
                    return result.Data?.Conversations ?? new List<OpenImConversationInfo>();
                }

                _logger.LogWarning("获取用户会话列表失败: {ErrCode} {ErrMsg}", result?.ErrCode, result?.ErrMsg);
                return new List<OpenImConversationInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户会话列表异常: UserId={UserId}", userId);
                return new List<OpenImConversationInfo>();
            }
        }
    }

    #region OpenIM 会话模型

    public class OpenImOwnerConversationResult
    {
        [JsonPropertyName("conversations")]
        public List<OpenImConversationInfo> Conversations { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class OpenImConversationInfo
    {
        [JsonPropertyName("conversationID")]
        public string ConversationID { get; set; }

        [JsonPropertyName("ownerUserID")]
        public string OwnerUserID { get; set; }

        [JsonPropertyName("userID")]
        public string UserID { get; set; }

        [JsonPropertyName("groupID")]
        public string GroupID { get; set; }

        [JsonPropertyName("conversationType")]
        public int ConversationType { get; set; }

        [JsonPropertyName("recvMsgOpt")]
        public int RecvMsgOpt { get; set; }

        [JsonPropertyName("unreadCount")]
        public int UnreadCount { get; set; }

        [JsonPropertyName("latestMsgSendTime")]
        public long LatestMsgSendTime { get; set; }

        [JsonPropertyName("isPinned")]
        public bool IsPinned { get; set; }
    }

    public class OpenImSortedConversationResult
    {
        [JsonPropertyName("conversationTotal")]
        public int ConversationTotal { get; set; }

        [JsonPropertyName("unreadTotal")]
        public int UnreadTotal { get; set; }

        [JsonPropertyName("conversationElems")]
        public List<OpenImConversationElem> ConversationElems { get; set; }
    }

    public class OpenImConversationElem
    {
        [JsonPropertyName("conversationID")]
        public string ConversationID { get; set; }

        [JsonPropertyName("conversationType")]
        public int ConversationType { get; set; }

        [JsonPropertyName("userID")]
        public string UserID { get; set; }

        [JsonPropertyName("groupID")]
        public string GroupID { get; set; }

        [JsonPropertyName("unreadCount")]
        public int UnreadCount { get; set; }

        [JsonPropertyName("msgInfo")]
        public OpenImMsgInfo MsgInfo { get; set; }
    }

    public class OpenImMsgInfo
    {
        [JsonPropertyName("serverMsgID")]
        public string ServerMsgID { get; set; }

        [JsonPropertyName("clientMsgID")]
        public string ClientMsgID { get; set; }

        [JsonPropertyName("sendID")]
        public string SendID { get; set; }

        [JsonPropertyName("recvID")]
        public string RecvID { get; set; }

        [JsonPropertyName("senderNickname")]
        public string SenderNickname { get; set; }

        [JsonPropertyName("senderFaceURL")]
        public string SenderFaceURL { get; set; }

        [JsonPropertyName("contentType")]
        public int ContentType { get; set; }

        [JsonPropertyName("sendTime")]
        public long SendTime { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    #endregion
}
