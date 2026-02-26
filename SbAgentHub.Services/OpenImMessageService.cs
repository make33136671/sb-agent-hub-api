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
            _apiUrl = config?.ApiUrl?.TrimEnd('/') ?? "";

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
    }
}
