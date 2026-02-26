using Microsoft.Extensions.Logging;
using SbAgentHub.Core;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models.Dto;
using System.Net.Http.Json;

namespace SbAgentHub.Services
{
    /// <summary>
    /// OpenIM 用户管理服务
    /// </summary>
    [AutoDependencyInjection]
    public class OpenImUserService
    {
        private readonly ILogger<OpenImUserService> _logger;
        private readonly OpenImAuthService _authService;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public OpenImUserService(ILogger<OpenImUserService> logger, OpenImAuthService authService)
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
        /// 注册用户到 OpenIM
        /// </summary>
        public async Task<bool> RegisterUserAsync(string userId, string nickname, string faceURL = "")
        {
            if (string.IsNullOrEmpty(_apiUrl)) return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/user/user_register");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    users = new[]
                    {
                        new OpenImUser
                        {
                            UserID = userId,
                            Nickname = nickname,
                            FaceURL = faceURL
                        }
                    }
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogInformation("OpenIM 用户注册成功: {UserId}", userId);
                    return true;
                }

                // 1110x 错误码通常表示用户已存在，也算成功
                if (result?.ErrCode >= 11001 && result?.ErrCode <= 11099)
                {
                    _logger.LogInformation("OpenIM 用户已存在: {UserId}", userId);
                    return true;
                }

                _logger.LogWarning("OpenIM 用户注册失败: {ErrCode} {ErrMsg}", result?.ErrCode, result?.ErrMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIM 用户注册异常: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        public async Task<bool> UpdateUserInfoAsync(string userId, string nickname = null, string faceURL = null)
        {
            if (string.IsNullOrEmpty(_apiUrl)) return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var updateData = new Dictionary<string, object>
                {
                    ["userID"] = userId
                };
                if (nickname != null) updateData["nickname"] = nickname;
                if (faceURL != null) updateData["faceURL"] = faceURL;

                var request = new HttpRequestMessage(HttpMethod.Post, "/user/update_user_info");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(updateData);

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                return result?.ErrCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新 OpenIM 用户信息异常: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 检查用户是否存在
        /// </summary>
        public async Task<bool> CheckUserExistsAsync(string userId)
        {
            if (string.IsNullOrEmpty(_apiUrl)) return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/user/get_users_info");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    userIDs = new[] { userId }
                });

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // 如果返回了用户信息则存在
                return content.Contains(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查 OpenIM 用户存在异常: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 注册用户并获取 Token
        /// </summary>
        public async Task<OpenImTokenData> RegisterAndGetTokenAsync(string userId, string nickname, string faceURL = "", int platformId = 5)
        {
            await RegisterUserAsync(userId, nickname, faceURL);
            return await _authService.GetUserTokenAsync(userId, platformId);
        }
    }
}
