using Microsoft.Extensions.Logging;
using SbAgentHub.Core;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models.Dto;
using System.Net.Http.Json;

namespace SbAgentHub.Services
{
    /// <summary>
    /// OpenIM 消息服务
    /// </summary>
    [AutoDependencyInjection]
    public class OpenImMessageService
    {
        private readonly ILogger<OpenImMessageService> _logger;
        private readonly OpenImAuthService _authService;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public OpenImMessageService(ILogger<OpenImMessageService> logger, OpenImAuthService authService)
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
        /// 发送系统文本消息（如欢迎语）
        /// </summary>
        public async Task<bool> SendTextMessageAsync(string fromUserId, string toUserId, string text)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(text)) return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/msg/send_msg");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    sendID = fromUserId,
                    recvID = toUserId,
                    contentType = 101, // Text
                    sessionType = 1,   // Single chat
                    content = new { text }
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogInformation("OpenIM 消息发送成功: {From} -> {To}", fromUserId, toUserId);
                    return true;
                }

                _logger.LogWarning("OpenIM 消息发送失败: {ErrCode} {ErrMsg}", result?.ErrCode, result?.ErrMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIM 发送消息异常: {From} -> {To}", fromUserId, toUserId);
                return false;
            }
        }

        /// <summary>
        /// 发送欢迎语
        /// </summary>
        public async Task<bool> SendWelcomeMessageAsync(string agentImUserId, string chatUserImUserId, string welcomeMessage)
        {
            if (string.IsNullOrEmpty(welcomeMessage)) return false;
            return await SendTextMessageAsync(agentImUserId, chatUserImUserId, welcomeMessage);
        }

        /// <summary>
        /// 发送系统自定义消息（contentType=110 CustomMessage）
        /// 类型常量定义在 SystemMessageTypes，与 openim-core-js SystemMessage.ts 对齐
        /// </summary>
        public async Task<bool> SendSystemMessageAsync(string fromUserId, string toUserId, string type, object payload, string description)
        {
            if (string.IsNullOrEmpty(_apiUrl)) return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var extensionData = new { systemMessage = true, notDisplay = true };

                var request = new HttpRequestMessage(HttpMethod.Post, "/msg/send_msg");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    sendID = fromUserId,
                    recvID = toUserId,
                    contentType = SystemMessageTypes.ContentType,
                    sessionType = 1, // Single chat
                    content = new
                    {
                        data = System.Text.Json.JsonSerializer.Serialize(payload),
                        extension = System.Text.Json.JsonSerializer.Serialize(extensionData),
                        description
                    }
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogInformation("OpenIM 系统消息发送成功: type={Type} {From} -> {To}", type, fromUserId, toUserId);
                    return true;
                }

                _logger.LogWarning("OpenIM 系统消息发送失败: type={Type} {ErrCode} {ErrMsg}", type, result?.ErrCode, result?.ErrMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIM 发送系统消息异常: type={Type} {From} -> {To}", type, fromUserId, toUserId);
                return false;
            }
        }

        /// <summary>
        /// 发送仅在线投递的系统消息（不持久化到历史记录）
        /// </summary>
        public async Task<bool> SendOnlineOnlySystemMessageAsync(string fromUserId, string toUserId, string type, object payload, string description)
        {
            if (string.IsNullOrEmpty(_apiUrl)) return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var extensionData = new { systemMessage = true, notDisplay = true };

                var request = new HttpRequestMessage(HttpMethod.Post, "/msg/send_msg");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    sendID = fromUserId,
                    recvID = toUserId,
                    contentType = SystemMessageTypes.ContentType,
                    sessionType = 1,
                    isOnlineOnly = true,
                    content = new
                    {
                        data = System.Text.Json.JsonSerializer.Serialize(payload),
                        extension = System.Text.Json.JsonSerializer.Serialize(extensionData),
                        description
                    }
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogDebug("在线系统消息发送成功: type={Type} {From} -> {To}", type, fromUserId, toUserId);
                    return true;
                }

                _logger.LogWarning("在线系统消息发送失败: type={Type} {ErrCode} {ErrMsg}", type, result?.ErrCode, result?.ErrMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "在线系统消息发送异常: type={Type} {From} -> {To}", type, fromUserId, toUserId);
                return false;
            }
        }

        /// <summary>
        /// 标记会话消息已读（调用 OpenIM Server API）
        /// </summary>
        public async Task<bool> MarkConversationAsReadAsync(string conversationId, string userId)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(conversationId)) return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/msg/mark_conversation_as_read");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    conversationID = conversationId,
                    userID = userId
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogDebug("标记会话已读成功: conv={ConvId} user={UserId}", conversationId, userId);
                    return true;
                }

                _logger.LogWarning("标记会话已读失败: {ErrCode} {ErrMsg}", result?.ErrCode, result?.ErrMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "标记会话已读异常: conv={ConvId} user={UserId}", conversationId, userId);
                return false;
            }
        }

        /// <summary>
        /// 发送会话创建消息（session_init）— 服务端分配客服后调用，确保 OpenIM 会话存在
        /// </summary>
        public async Task<bool> SendSessionInitMessageAsync(string fromUserId, string toUserId, string reason = "login")
        {
            var payload = new
            {
                type = SystemMessageTypes.SessionInit,
                visitorId = toUserId,
                reason,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            return await SendSystemMessageAsync(fromUserId, toUserId, SystemMessageTypes.SessionInit, payload, "[会话已建立]");
        }
    }
}
