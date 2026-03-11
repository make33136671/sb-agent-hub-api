using Microsoft.Extensions.Logging;
using SbAgentHub.Core;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Models.Dto;
using System.Net.Http.Json;

namespace SbAgentHub.Services
{
    /// <summary>
    /// OpenIM 群组服务 — 创建/邀请/踢人/解散群组
    /// </summary>
    [AutoDependencyInjection]
    public class OpenImGroupService
    {
        private readonly ILogger<OpenImGroupService> _logger;
        private readonly OpenImAuthService _authService;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public OpenImGroupService(ILogger<OpenImGroupService> logger, OpenImAuthService authService)
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
        /// 创建群组（WorkGroup 类型，静默通知）
        /// </summary>
        /// <param name="groupName">群组名称</param>
        /// <param name="ownerUserId">群主 OpenIM userID</param>
        /// <param name="memberUserIds">初始成员 OpenIM userIDs（不含群主）</param>
        /// <returns>群组 ID，失败返回 null</returns>
        public async Task<string> CreateGroupAsync(string groupName, string ownerUserId, List<string> memberUserIds)
        {
            if (string.IsNullOrEmpty(_apiUrl)) return null;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/group/create_group");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    ownerUserID = ownerUserId,
                    groupInfo = new
                    {
                        groupName,
                        groupType = 2, // WorkGroup（邀请制，免审批）
                        // 静默模式：不产生入群/退群系统消息
                        needVerification = 0,
                        lookMemberInfo = 0,
                        applyMemberFriend = 0,
                        notification = "",
                    },
                    memberUserIDs = memberUserIds,
                    adminUserIDs = new string[] { }
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<GroupCreateData>>();

                if (result?.ErrCode == 0 && result.Data?.GroupInfo != null)
                {
                    var groupId = result.Data.GroupInfo.GroupID;
                    _logger.LogInformation("OpenIM 群组创建成功: GroupId={GroupId}, Owner={Owner}, Members={Count}",
                        groupId, ownerUserId, memberUserIds.Count);
                    return groupId;
                }

                _logger.LogWarning("OpenIM 群组创建失败: {ErrCode} {ErrMsg}", result?.ErrCode, result?.ErrMsg);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIM 创建群组异常: Owner={Owner}", ownerUserId);
                return null;
            }
        }

        /// <summary>
        /// 邀请用户加入群组
        /// </summary>
        public async Task<bool> InviteToGroupAsync(string groupId, List<string> userIds)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(groupId) || userIds == null || userIds.Count == 0)
                return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/group/invite_user_to_group");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    groupID = groupId,
                    invitedUserIDs = userIds,
                    reason = ""
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogInformation("OpenIM 邀请入群成功: GroupId={GroupId}, Users={Users}",
                        groupId, string.Join(",", userIds));
                    return true;
                }

                _logger.LogWarning("OpenIM 邀请入群失败: GroupId={GroupId}, {ErrCode} {ErrMsg}",
                    groupId, result?.ErrCode, result?.ErrMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIM 邀请入群异常: GroupId={GroupId}", groupId);
                return false;
            }
        }

        /// <summary>
        /// 将用户从群组移除
        /// </summary>
        public async Task<bool> KickFromGroupAsync(string groupId, List<string> userIds)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(groupId) || userIds == null || userIds.Count == 0)
                return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/group/kick_group_member");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    groupID = groupId,
                    kickedUserIDs = userIds,
                    reason = ""
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogDebug("OpenIM 踢出群成员成功: GroupId={GroupId}, Users={Users}",
                        groupId, string.Join(",", userIds));
                    return true;
                }

                _logger.LogWarning("OpenIM 踢出群成员失败: GroupId={GroupId}, {ErrCode} {ErrMsg}",
                    groupId, result?.ErrCode, result?.ErrMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIM 踢出群成员异常: GroupId={GroupId}", groupId);
                return false;
            }
        }

        /// <summary>
        /// 解散群组
        /// </summary>
        public async Task<bool> DismissGroupAsync(string groupId)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(groupId))
                return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/group/dismiss_group");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    groupID = groupId
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();

                if (result?.ErrCode == 0)
                {
                    _logger.LogInformation("OpenIM 群组解散成功: GroupId={GroupId}", groupId);
                    return true;
                }

                _logger.LogWarning("OpenIM 群组解散失败: GroupId={GroupId}, {ErrCode} {ErrMsg}",
                    groupId, result?.ErrCode, result?.ErrMsg);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIM 解散群组异常: GroupId={GroupId}", groupId);
                return false;
            }
        }

        /// <summary>
        /// 设置群组消息免打扰（静默模式，不产生入群/退群系统消息）
        /// </summary>
        public async Task<bool> MuteGroupNotificationsAsync(string groupId)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(groupId))
                return false;

            try
            {
                string adminToken = await _authService.GetAdminTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/group/set_group_info");
                request.Headers.Add("operationID", Guid.NewGuid().ToString());
                request.Headers.Add("token", adminToken);
                request.Content = JsonContent.Create(new
                {
                    groupInfoForSet = new
                    {
                        groupID = groupId,
                        needVerification = 0,
                        lookMemberInfo = 0,
                        applyMemberFriend = 0,
                    }
                });

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadFromJsonAsync<OpenImResponse<object>>();
                return result?.ErrCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenIM 设置群静默模式异常: GroupId={GroupId}", groupId);
                return false;
            }
        }
    }

    /// <summary>
    /// 群组创建返回数据（OpenIM 返回格式: data.groupInfo.groupID）
    /// </summary>
    public class GroupCreateData
    {
        [System.Text.Json.Serialization.JsonPropertyName("groupInfo")]
        public GroupInfoData GroupInfo { get; set; }
    }

    public class GroupInfoData
    {
        [System.Text.Json.Serialization.JsonPropertyName("groupID")]
        public string GroupID { get; set; }
    }
}
