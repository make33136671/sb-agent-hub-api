using Microsoft.AspNetCore.Http;
using Sb.Apps.AgentHub.Core;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Models.Dto;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// SDK 响应构建服务 — 将访客认证结果映射为 realtime-core-js SDK 期望的嵌套响应格式，
    /// 并构建 Realtime 连接配置、功能开关与访客表单配置。
    /// 从 ChatAuthService 拆分，聚焦 SDK 响应映射职责。
    /// </summary>
    [AutoDependencyInjection]
    public class SdkResponseBuilder
    {
        private readonly ChatAuthService _chatAuthService;
        private readonly ChatUserService _chatUserService;
        private readonly CustomerServiceConfigService _configService;
        private readonly QuotaService _quotaService;
        private readonly VisitorFormService _visitorFormService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SdkResponseBuilder(
            ChatAuthService chatAuthService,
            ChatUserService chatUserService,
            CustomerServiceConfigService configService,
            QuotaService quotaService,
            VisitorFormService visitorFormService,
            IHttpContextAccessor httpContextAccessor)
        {
            _chatAuthService = chatAuthService;
            _chatUserService = chatUserService;
            _configService = configService;
            _quotaService = quotaService;
            _visitorFormService = visitorFormService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// SDK 端获取 Token（适配 realtime-core-js SDK 的接口格式）。
        /// 映射为 VisitorAuthRequest 调用 ChatAuthService 核心认证逻辑后，再映射为 SDK 期望的嵌套格式。
        /// </summary>
        public async Task<ResultData> GetTokenForSdkAsync(GetTokenFormRequest request,
            string requestDomain = "", string ipAddress = "", string userAgent = "")
        {
            // 映射为 VisitorAuthRequest
            var visitorRequest = new VisitorAuthRequest
            {
                PlatformKey = request.PlatformKey,
                ClientId = request.ClientId,
                ExternalUserId = request.UserId,
                Username = request.UserId,
                Nickname = request.Nickname,
                Avatar = request.Avatar,
                DeviceType = request.PlatformType.ToString(),
                Source = request.Source,
                RequestDomain = requestDomain,
                IpAddress = ipAddress,
                UserAgent = userAgent,
            };

            // 调用核心认证逻辑
            var result = await _chatAuthService.VisitorAuthAsync(visitorRequest);
            if (result.Code != 0) return result;

            var authResponse = result.Data as VisitorAuthResponse;
            if (authResponse == null) return ResultData.No("认证失败!");

            // 查询 ChatUser 获取更多信息
            var chatUser = _chatUserService.GetByChatUserId(authResponse.ChatUserId);

            // 映射为 SDK 期望的嵌套格式
            var sdkResponse = new GetTokenSdkResponse
            {
                UserID = authResponse.ChatUserId,
                Token = authResponse.Token,
                ExpireTime = authResponse.TokenExpire,
                User = new SdkUserProfile
                {
                    UserId = authResponse.ChatUserId,
                    UserType = chatUser != null ? (int)chatUser.UserType : 0,
                    IsNewUser = false,
                    Nickname = authResponse.Nickname ?? "访客",
                    Avatar = authResponse.Avatar ?? "",
                    ExternalUserId = request.UserId,
                    DeviceId = request.ClientId,
                },
                Agent = authResponse.AgentUserId != null ? new SdkAgentInfo
                {
                    AgentUserId = authResponse.AgentUserId,
                    Nickname = authResponse.AgentName ?? "客服",
                    Avatar = authResponse.AgentAvatar.ToAbsoluteUrl(_httpContextAccessor.HttpContext!),
                    OnlineStatus = authResponse.AgentOnline ? 1 : 0,
                    IsOnline = authResponse.AgentOnline,
                    WelcomeMessage = authResponse.WelcomeMessage,
                    AutoSendWelcome = !string.IsNullOrEmpty(authResponse.WelcomeMessage),
                    ConversationId = authResponse.GroupId,
                    GroupId = authResponse.GroupId,
                } : null!,
                Platform = new SdkPlatformInfo
                {
                    PlatformId = chatUser?.PlatformId ?? 0,
                    PlatformName = authResponse.PlatformName ?? "",
                    OfflineMessage = authResponse.OfflineMessage,
                    AllowOfflineMessage = true,
                },
                Features = BuildFeatureFlags(chatUser?.OwnerId ?? 0),
                Queue = authResponse.IsQueued ? new SdkQueueInfo
                {
                    IsQueued = true,
                    Position = authResponse.QueuePosition,
                    TotalWaiting = authResponse.QueueWaitingCount,
                    EstimatedWaitMinutes = authResponse.EstimatedWaitMinutes
                } : null!,
                VisitorForm = BuildVisitorForm(chatUser?.OwnerId ?? 0, chatUser?.PlatformId ?? 0)!,
                RealtimeConfig = BuildRealtimeConfig(_httpContextAccessor.HttpContext)
            };

            return ResultData.Ok(sdkResponse);
        }

        /// <summary>
        /// 构建 Realtime 连接配置（下发给客户端连接 Realtime Hub）。
        /// 静态实现以避免与 ChatAuthService 形成构造函数循环依赖。
        /// </summary>
        public static SdkRealtimeConfig BuildRealtimeConfig(HttpContext? context)
        {
            var baseUrl = GetRequestBaseUrl(context);
            var wsScheme = baseUrl.StartsWith("https") ? "wss" : "ws";
            return new SdkRealtimeConfig
            {
                ApiUrl = baseUrl,
                WsUrl = $"{wsScheme}{baseUrl.Substring(baseUrl.IndexOf("://"))}/hubs/realtime"
            };
        }

        /// <summary>
        /// 获取当前请求的基础 URL（scheme://host）。
        /// 优先级：ForceHttps 配置 > X-Forwarded-Proto 头 > Request.Scheme
        /// </summary>
        public static string GetRequestBaseUrl(HttpContext? context)
        {
            if (context == null) return "";

            var request = context.Request;

            // 1. 最高优先级：ForceHttps 配置强制使用 https
            if (AppGlobal.AppSettings?.ForceHttps == true)
            {
                return $"https://{request.Host}";
            }

            // 2. 读取 X-Forwarded-Proto（Nginx 等反向代理会设置此头）
            string scheme = request.Scheme;
            if (request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto)
                && !string.IsNullOrEmpty(forwardedProto))
            {
                // 可能是逗号分隔的多级代理值，取第一个
                scheme = forwardedProto.ToString().Split(',')[0].Trim().ToLowerInvariant();
            }

            return $"{scheme}://{request.Host}";
        }

        /// <summary>
        /// 构建功能开关
        /// </summary>
        private SdkFeatureFlags BuildFeatureFlags(int ownerId)
        {
            if (ownerId <= 0) return new SdkFeatureFlags();
            var csConfig = _configService.GetEffectiveConfig(ownerId);
            return new SdkFeatureFlags
            {
                EnableSatisfactionRating = _quotaService.CheckFeature(ownerId, "feature_satisfaction"),
                EnableVisitorForm = _quotaService.CheckFeature(ownerId, "feature_visitor_form"),
                EnableQueue = csConfig.EnableQueue,
                EnableBroadcast = csConfig.EnableBroadcast,
            };
        }

        /// <summary>
        /// 构建访客表单配置
        /// </summary>
        private SdkVisitorFormConfig? BuildVisitorForm(int ownerId, int platformId)
        {
            if (ownerId <= 0) return null;
            if (!_quotaService.CheckFeature(ownerId, "feature_visitor_form")) return null;

            var form = _visitorFormService.GetActiveForm(ownerId, platformId);
            if (form == null) return null;

            return new SdkVisitorFormConfig
            {
                FormId = form.Id,
                FormName = form.FormName,
                FormFields = form.FormFields,
                IsRequired = form.IsRequired
            };
        }
    }
}
