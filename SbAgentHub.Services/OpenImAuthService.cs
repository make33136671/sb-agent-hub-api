using Microsoft.Extensions.Logging;
using SbAgentHub.Core;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models.Dto;
using System.Net.Http.Json;
using System.Text.Json;

namespace SbAgentHub.Services
{
    /// <summary>
    /// OpenIM 认证服务 — Token 管理
    /// </summary>
    [AutoDependencyInjection]
    public class OpenImAuthService
    {
        private readonly ILogger<OpenImAuthService> _logger;
        private readonly ICacheService _cacheService;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _secret;
        private readonly string _adminUserId;

        private const string ADMIN_TOKEN_CACHE_KEY = "openim:admin:token";

        public OpenImAuthService(ILogger<OpenImAuthService> logger, ICacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;

            var config = AppGlobal.AppSettings?.OpenIM;
            _apiUrl = config?.ImApiUrl?.TrimEnd('/') ?? "";
            _secret = config?.Secret ?? "";
            _adminUserId = config?.AdminUserId ?? "imAdmin";

            _httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(_apiUrl))
            {
                _httpClient.BaseAddress = new Uri(_apiUrl);
            }
        }

        /// <summary>
        /// 获取管理员 Token（缓存）
        /// </summary>
        public async Task<string> GetAdminTokenAsync()
        {
            var cached = _cacheService.Get<string>(ADMIN_TOKEN_CACHE_KEY);
            if (!string.IsNullOrEmpty(cached)) return cached;

            var token = await RequestAdminTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                // 缓存 55 分钟（Token 有效期通常 1 小时）
                _cacheService.Set(ADMIN_TOKEN_CACHE_KEY, token, TimeSpan.FromMinutes(55));
            }
            return token;
        }

        /// <summary>
        /// 获取用户 Token
        /// </summary>
        public async Task<OpenImTokenData> GetUserTokenAsync(string userId, int platformId = 5)
        {
            if (string.IsNullOrEmpty(_apiUrl))
            {
                _logger.LogWarning("OpenIM API URL 未配置");
                return null;
            }

            try
            {
                var (result, responseBody) = await DoGetUserTokenAsync(userId, platformId);

                if (result?.ErrCode == 0)
                {
                    return result.Data;
                }

                // 1507 TokenNotExistError：管理员 Token 失效，清除缓存后重试一次
                if (result?.ErrCode == 1507)
                {
                    _logger.LogWarning("OpenIM 管理员 Token 已失效，清除缓存后重试获取用户 Token: UserId={UserId}", userId);
                    InvalidateAdminToken();
                    (result, responseBody) = await DoGetUserTokenAsync(userId, platformId);

                    if (result?.ErrCode == 0)
                    {
                        return result.Data;
                    }
                }

                _logger.LogWarning("获取 OpenIM 用户 Token 业务失败: UserId={UserId}, ErrCode={ErrCode}, ErrMsg={ErrMsg}",
                    userId, result?.ErrCode, result?.ErrMsg);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 OpenIM 用户 Token 异常: UserId={UserId}, ApiUrl={ApiUrl}", userId, _apiUrl);
                return null;
            }
        }

        private async Task<(OpenImResponse<OpenImTokenData> result, string responseBody)> DoGetUserTokenAsync(string userId, int platformId)
        {
            string adminToken = await GetAdminTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, "/auth/get_user_token");
            request.Headers.Add("operationID", Guid.NewGuid().ToString());
            request.Headers.Add("token", adminToken);
            request.Content = JsonContent.Create(new
            {
                secret = _secret,
                platformID = platformId,
                userID = userId
            });

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("获取 OpenIM 用户 Token HTTP 失败: UserId={UserId}, StatusCode={StatusCode}, Body={Body}",
                    userId, (int)response.StatusCode, responseBody);
                return (null, responseBody);
            }

            var result = JsonSerializer.Deserialize<OpenImResponse<OpenImTokenData>>(responseBody);
            return (result, responseBody);
        }

        /// <summary>
        /// 强制踢掉用户旧会话（调用 OpenIM /auth/force_logout）
        /// </summary>
        public async Task ForceLogoutAsync(string userId, int platformId = 5)
        {
            if (string.IsNullOrEmpty(_apiUrl))
            {
                _logger.LogWarning("OpenIM API URL 未配置，跳过 ForceLogout");
                return;
            }

            try
            {
                string adminToken = await GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/auth/force_logout");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    platformID = platformId,
                    userID = userId
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogInformation("ForceLogout 成功: UserId={UserId}, PlatformId={PlatformId}", userId, platformId);
                }
                else
                {
                    _logger.LogWarning("ForceLogout 失败: UserId={UserId}, ErrMsg={ErrMsg}", userId, result?.ErrMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForceLogout 异常: UserId={UserId}", userId);
            }
        }

        /// <summary>
        /// 清除管理员 Token 缓存
        /// </summary>
        public void InvalidateAdminToken()
        {
            _cacheService.Remove(ADMIN_TOKEN_CACHE_KEY);
        }

        #region Private

        private async Task<string> RequestAdminTokenAsync()
        {
            if (string.IsNullOrEmpty(_apiUrl))
            {
                _logger.LogWarning("OpenIM API URL 未配置");
                return null;
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/auth/get_admin_token");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Content = JsonContent.Create(new
                {
                    secret = _secret,
                    userID = _adminUserId
                });

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("获取 OpenIM 管理员 Token HTTP 失败: StatusCode={StatusCode}, Body={Body}",
                        (int)response.StatusCode, responseBody);
                    return null;
                }

                var result = JsonSerializer.Deserialize<OpenImResponse<OpenImTokenData>>(responseBody);

                if (result?.ErrCode == 0)
                {
                    _logger.LogInformation("获取 OpenIM 管理员 Token 成功");
                    return result.Data?.Token;
                }

                _logger.LogWarning("获取 OpenIM 管理员 Token 业务失败: ErrCode={ErrCode}, ErrMsg={ErrMsg}",
                    result?.ErrCode, result?.ErrMsg);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 OpenIM 管理员 Token 异常: ApiUrl={ApiUrl}", _apiUrl);
                return null;
            }
        }

        #endregion
    }
}
