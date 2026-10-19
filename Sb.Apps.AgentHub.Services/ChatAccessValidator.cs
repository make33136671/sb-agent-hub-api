using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Models.Dto;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 聊天访问校验服务 — 负责平台接入来源/域名白名单校验与访客登录记录。
    /// 从 ChatAuthService 拆分，聚焦访问校验职责。
    /// </summary>
    [AutoDependencyInjection]
    public class ChatAccessValidator
    {
        private readonly ILogger<ChatAccessValidator> _logger;
        private readonly AccessViolationService _violationService;
        private readonly PendingDomainService _pendingDomainService;
        private readonly ChatUserService _chatUserService;

        public ChatAccessValidator(
            ILogger<ChatAccessValidator> logger,
            AccessViolationService violationService,
            PendingDomainService pendingDomainService,
            ChatUserService chatUserService)
        {
            _logger = logger;
            _violationService = violationService;
            _pendingDomainService = pendingDomainService;
            _chatUserService = chatUserService;
        }

        /// <summary>
        /// 校验接入来源与域名白名单。
        /// 返回 null 表示校验通过；返回 ResultData 表示拒绝（含拒绝原因）。
        /// </summary>
        public ResultData? ValidateAccess(ChatPlatform platform, VisitorAuthRequest request)
        {
            string source = request.Source;

            // 兼容旧版本（不带 source 参数）：默认 js
            if (string.IsNullOrEmpty(source))
                return null;

            // 接入类型校验已去除：即使接入类型不一致，客户端仍可上线
            // 仅记录日志，不阻断连接
            string expectedSource = platform.AccessType.ToString().ToLower();
            if (source.ToLower() != expectedSource)
            {
                _violationService.Log(platform.Id, platform.PlatformKey, platform.OwnerId,
                    request.RequestDomain, source, expectedSource,
                    "type_mismatch", request.IpAddress, request.UserAgent);

                _logger.LogWarning("接入类型不匹配（已放行）: PlatformKey={PlatformKey}, Expected={Expected}, Got={Got}",
                    platform.PlatformKey, expectedSource, source);
            }

            // 域名白名单校验（Chat 类型豁免）
            if (platform.AccessType != AccessType.Chat && !string.IsNullOrEmpty(request.RequestDomain))
            {
                if (!IsDomainAllowed(platform, request.RequestDomain))
                {
                    _violationService.Log(platform.Id, platform.PlatformKey, platform.OwnerId,
                        request.RequestDomain, source, expectedSource,
                        "domain_mismatch", request.IpAddress, request.UserAgent);

                    // 将未知域名加入待审核列表
                    _pendingDomainService.AddOrUpdate(platform.Id, platform.OwnerId,
                        request.RequestDomain, request.IpAddress, request.UserAgent);

                    _logger.LogWarning("域名不匹配: PlatformKey={PlatformKey}, Domain={Domain}",
                        platform.PlatformKey, request.RequestDomain);

                    return ResultData.No("接入域名未授权，请在平台设置中添加域名白名单!");
                }
            }

            return null; // 校验通过
        }

        /// <summary>
        /// 检查域名是否在白名单中
        /// </summary>
        public bool IsDomainAllowed(ChatPlatform platform, string requestDomain)
        {
            // 如果没有配置白名单，允许所有域名
            if (string.IsNullOrEmpty(platform.AllowedDomains) && string.IsNullOrEmpty(platform.Domain))
                return true;

            requestDomain = requestDomain.ToLower().Trim();

            // 优先检查 AllowedDomains（JSON 数组）
            if (!string.IsNullOrEmpty(platform.AllowedDomains))
            {
                try
                {
                    var domains = System.Text.Json.JsonSerializer.Deserialize<string[]>(platform.AllowedDomains);
                    if (domains != null && domains.Any(d => d.ToLower().Trim() == requestDomain))
                        return true;
                }
                catch
                {
                    // JSON 解析失败，忽略
                }
            }

            // 兼容旧 Domain 字段
            if (!string.IsNullOrEmpty(platform.Domain))
            {
                if (platform.Domain.ToLower().Trim() == requestDomain)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 记录访客登录（Realtime 会话登录留痕）
        /// </summary>
        public void RecordLogin(ChatUser chatUser, VisitorAuthRequest request)
        {
            _chatUserService.RecordLogin(chatUser.Id, chatUser.PlatformId,
                request.ClientId, request.DeviceType, request.UserAgent, request.IpAddress);
        }
    }
}
