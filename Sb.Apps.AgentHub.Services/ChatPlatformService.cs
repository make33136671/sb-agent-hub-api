using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 接入平台管理服务
    /// </summary>
    [AutoDependencyInjection]
    public class ChatPlatformService : BaseCrudService<ChatPlatform>
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QuotaService _quotaService;
        private readonly PendingDomainService _pendingDomainService;
        private readonly MemberService _memberService;
        private readonly AdminService _adminService;

        public ChatPlatformService(IFreeSql fsql, ILogger<ChatPlatformService> logger,
            IHttpContextAccessor httpContextAccessor, QuotaService quotaService,
            PendingDomainService pendingDomainService,
            MemberService memberService, AdminService adminService) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _quotaService = quotaService;
            _pendingDomainService = pendingDomainService;
            _memberService = memberService;
            _adminService = adminService;
        }

        /// <summary>
        /// 平台列表（Admin 看全部，Owner 看自己的）
        /// </summary>
        public ResultData GetList(string keyword = "", int? ownerId = null, PlatformType? platformType = null,
            bool? isEnabled = null, string field = "", string order = "",
            int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            string userType = user.UserType();
            string role = user.Role();

            var query = Select;

            // Owner 只能看自己的平台
            if (userType == "Member" && role == "Owner")
            {
                int currentUserId = user.Id();
                query = query.Where(c => c.OwnerId == currentUserId);
            }

            query = query.WhereIf(!string.IsNullOrEmpty(keyword),
                c => c.PlatformName.Contains(keyword) || c.PlatformKey.Contains(keyword));
            query = query.WhereIf(ownerId.HasValue, c => c.OwnerId == ownerId!.Value);
            query = query.WhereIf(platformType.HasValue, c => c.PlatformType == platformType!.Value);
            query = query.WhereIf(isEnabled.HasValue, c => c.IsEnabled == isEnabled!.Value);

            query = ApplySorting(query, field, order);

            var platforms = query.Count(out long dataCount).Page(page, pageSize).ToList(c => new
            {
                c.Id, c.OwnerId, c.PlatformName, c.Logo, c.PlatformType, c.AccessType, c.PlatformKey,
                c.Domain, c.AllowedDomains, c.IsEnabled, c.WelcomeMessage, c.CreateTime,
                c.IsAdminTakeover, c.TakeoverAdminId
            });

            // 批量查询关联名称
            var ownerIds = platforms.Where(p => p.OwnerId > 0).Select(p => p.OwnerId).Distinct().ToList();
            var adminIds = platforms.Where(p => p.TakeoverAdminId.HasValue).Select(p => p.TakeoverAdminId!.Value).Distinct().ToList();

            var ownerNames = _memberService.GetMemberNamesByIds(ownerIds);
            var adminNames = _adminService.GetAdminNamesByIds(adminIds);

            // 批量查询待审核域名数量
            var allPlatformIds = platforms.Select(p => p.Id).ToList();
            var pendingCounts = _pendingDomainService.GetPendingCountsByPlatforms(allPlatformIds);

            var data = platforms.Select(p => new
            {
                p.Id, p.OwnerId, p.PlatformName, p.Logo, p.PlatformType, p.AccessType, p.PlatformKey,
                p.Domain, p.AllowedDomains, p.IsEnabled, p.WelcomeMessage, p.CreateTime,
                p.IsAdminTakeover, p.TakeoverAdminId,
                AccessTypeName = GetAccessTypeName(p.AccessType),
                OwnerName = p.OwnerId > 0 && ownerNames.ContainsKey(p.OwnerId) ? ownerNames[p.OwnerId] : "",
                TakeoverAdminName = p.TakeoverAdminId.HasValue && adminNames.ContainsKey(p.TakeoverAdminId.Value)
                    ? adminNames[p.TakeoverAdminId.Value] : "",
                PendingDomainCount = pendingCounts.GetValueOrDefault(p.Id)
            });

            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 创建接入平台
        /// </summary>
        public ResultData Create(ChatPlatform platform)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            string userType = user.UserType();

            if (string.IsNullOrEmpty(platform.PlatformName))
                return ResultData.No("平台名称不能为空!");

            // PlatformType 和 AccessType 必选
            if (string.IsNullOrEmpty(platform.PlatformType.ToString()) || !Enum.IsDefined(typeof(PlatformType), platform.PlatformType))
                return ResultData.No("平台类型不能为空!");
            if (!Enum.IsDefined(typeof(AccessType), platform.AccessType))
                return ResultData.No("接入方式不能为空!");

            // 防御性编程：OwnerId 由后端根据当前用户自动设置
            if (userType == "Member")
            {
                // 站长操作：锁死 OwnerId 为当前用户 Id，忽略前端传入值
                platform.OwnerId = user.Id();
            }
            // Admin 操作：允许前端指定 OwnerId

            // 配额校验
            if (platform.OwnerId > 0)
            {
                var quotaCheck = _quotaService.CheckQuota(platform.OwnerId, "max_platforms");
                if (!quotaCheck.IsAvailable)
                    return ResultData.No($"平台配额不足! 已使用 {quotaCheck.Used}/{quotaCheck.Limit}");
            }

            // 自动格式化域名
            platform.Domain = PendingDomainService.NormalizeDomain(platform.Domain);
            platform.AllowedDomains = PendingDomainService.FormatDomains(platform.AllowedDomains);

            // 自动生成 PlatformKey
            platform.PlatformKey = GeneratePlatformKey();
            platform.CreateTime = DateTime.Now;
            platform.IsDeleted = false;

            platform = Insert(platform);

            return ResultData.Ok("平台创建成功!", new
            {
                platform.Id, platform.PlatformName, platform.Logo, platform.PlatformKey,
                platform.PlatformType, platform.AccessType, platform.OwnerId
            });
        }

        /// <summary>
        /// 编辑接入平台
        /// </summary>
        public ResultData Edit(ChatPlatform platform)
        {
            var existing = Select.Where(c => c.Id == platform.Id).First();
            if (existing == null) return ResultData.No("平台不存在!");

            // Owner 权限校验
            if (!HasOwnership(existing))
                return ResultData.No("无权操作此平台!");

            var user = _httpContextAccessor.HttpContext!.User;
            string userType = user.UserType();

            // PlatformType 和 AccessType 必选
            if (!Enum.IsDefined(typeof(PlatformType), platform.PlatformType))
                return ResultData.No("平台类型不能为空!");
            if (!Enum.IsDefined(typeof(AccessType), platform.AccessType))
                return ResultData.No("接入方式不能为空!");

            existing.PlatformName = platform.PlatformName;
            existing.Logo = platform.Logo;
            existing.PlatformType = platform.PlatformType;
            existing.AccessType = platform.AccessType;
            existing.Domain = PendingDomainService.NormalizeDomain(platform.Domain);
            existing.AllowedDomains = PendingDomainService.FormatDomains(platform.AllowedDomains);
            existing.WelcomeMessage = platform.WelcomeMessage;

            // 防御性编程：只有 Admin 可修改 OwnerId
            if (userType == "Admin")
            {
                existing.OwnerId = platform.OwnerId;
                existing.IsAdminTakeover = platform.IsAdminTakeover;
                existing.TakeoverAdminId = platform.TakeoverAdminId;
            }
            // Member 操作：OwnerId 不可修改

            UpdateDiy.UpdateColumns(c => new
            {
                c.PlatformName, c.Logo, c.PlatformType, c.AccessType, c.Domain, c.AllowedDomains,
                c.WelcomeMessage, c.OwnerId, c.IsAdminTakeover, c.TakeoverAdminId
            }).SetSource(existing).ExecuteAffrows();

            return ResultData.Ok("平台更新成功!");
        }

        /// <summary>
        /// 重置平台秘钥
        /// </summary>
        public ResultData ResetPlatformKey(int id)
        {
            var platform = Select.Where(c => c.Id == id).First();
            if (platform == null) return ResultData.No("平台不存在!");
            if (!HasOwnership(platform)) return ResultData.No("无权操作此平台!");

            platform.PlatformKey = GeneratePlatformKey();
            UpdateDiy.UpdateColumns(c => c.PlatformKey).SetSource(platform).ExecuteAffrows();

            return ResultData.Ok("秘钥已重置!", new { platform.PlatformKey });
        }

        /// <summary>
        /// 切换平台启用状态
        /// </summary>
        public ResultData ToggleEnabled(int id)
        {
            var platform = Select.Where(c => c.Id == id).First();
            if (platform == null) return ResultData.No("平台不存在!");
            if (!HasOwnership(platform)) return ResultData.No("无权操作此平台!");

            platform.IsEnabled = !platform.IsEnabled;
            UpdateDiy.UpdateColumns(c => c.IsEnabled).SetSource(platform).ExecuteAffrows();

            return ResultData.Ok("状态切换成功!", new { platform.IsEnabled });
        }

        /// <summary>
        /// 获取当前 Owner 的平台列表（简化）
        /// </summary>
        public ResultData GetMyPlatforms()
        {
            int userId = _httpContextAccessor.HttpContext!.User.Id();

            var platforms = Select.Where(c => c.OwnerId == userId)
                .OrderByDescending(c => c.Id)
                .ToList(c => new
                {
                    c.Id, c.PlatformName, c.PlatformType, c.PlatformKey,
                    c.Domain, c.IsEnabled, c.CreateTime
                });

            return ResultData.Ok(platforms);
        }

        /// <summary>
        /// 获取所有启用的平台列表
        /// </summary>
        public ResultData GetEnabledPlatforms()
        {
            var platforms = Select.Where(c => c.IsEnabled)
                .OrderByDescending(c => c.Id)
                .ToList(c => new
                {
                    c.Id, c.PlatformName, c.PlatformType, c.PlatformKey, c.OwnerId
                });

            return ResultData.Ok(platforms);
        }

        /// <summary>
        /// 通过 PlatformKey 获取平台
        /// </summary>
        public ChatPlatform GetByPlatformKey(string platformKey)
        {
            return Select.Where(c => c.PlatformKey == platformKey && c.IsEnabled).First();
        }

        /// <summary>
        /// 按主键获取平台（仓储访问入口，供其他 Service 调用）
        /// </summary>
        public ChatPlatform GetById(int id)
        {
            return Select.Where(c => c.Id == id).First();
        }

        /// <summary>
        /// 统计站长下的平台数量（配额 max_platforms 用）
        /// </summary>
        public int CountByOwner(int ownerId)
        {
            return (int)Select.Where(c => c.OwnerId == ownerId && !c.IsDeleted).Count();
        }

        /// <summary>
        /// 获取站长下所有平台 Id（每日访客统计去重用）
        /// </summary>
        public List<int> GetPlatformIdsByOwner(int ownerId)
        {
            return Select.Where(c => c.OwnerId == ownerId && !c.IsDeleted)
                .ToList(c => c.Id);
        }

        /// <summary>
        /// 按平台 Id 批量获取平台名称映射
        /// </summary>
        public Dictionary<int, string> GetPlatformNamesByIds(List<int> platformIds)
        {
            if (platformIds == null || !platformIds.Any())
                return new Dictionary<int, string>();

            return Select.Where(c => platformIds.Contains(c.Id))
                .ToDictionary(c => c.Id, c => c.PlatformName ?? "");
        }

        /// <summary>
        /// 更新平台白名单域名（供 PendingDomainService.Approve 调用）
        /// </summary>
        public void UpdateAllowedDomains(int platformId, string allowedDomains)
        {
            UpdateDiy.Where(c => c.Id == platformId)
                .Set(c => c.AllowedDomains, allowedDomains)
                .ExecuteAffrows();
        }

        /// <summary>
        /// 获取站长选项列表
        /// </summary>
        public ResultData GetOwnerOptions()
        {
            var owners = _memberService.GetActiveOwners()
                .Select(m => new { m.Id, m.Name, m.Username });
            return ResultData.Ok(owners);
        }

        /// <summary>
        /// 获取管理员选项列表
        /// </summary>
        public ResultData GetAdminOptions()
        {
            var admins = _adminService.GetActiveAdmins()
                .Select(a => new { a.Id, a.Name, a.Username });
            return ResultData.Ok(admins);
        }

        /// <summary>
        /// 切换管理员接管状态
        /// </summary>
        public ResultData ToggleTakeover(int id, int? adminId)
        {
            var platform = Select.Where(c => c.Id == id).First();
            if (platform == null) return ResultData.No("平台不存在!");

            if (platform.IsAdminTakeover)
            {
                // 关闭接管
                platform.IsAdminTakeover = false;
                platform.TakeoverAdminId = null;
            }
            else
            {
                // 开启接管
                if (!adminId.HasValue || adminId.Value <= 0)
                    return ResultData.No("请选择接管管理员!");
                platform.IsAdminTakeover = true;
                platform.TakeoverAdminId = adminId.Value;
            }

            UpdateDiy.UpdateColumns(c => new { c.IsAdminTakeover, c.TakeoverAdminId })
                .SetSource(platform).ExecuteAffrows();

            return ResultData.Ok(platform.IsAdminTakeover ? "已开启接管" : "已取消接管",
                new { platform.IsAdminTakeover, platform.TakeoverAdminId });
        }

        /// <summary>
        /// 获取接入代码/链接
        /// </summary>
        public ResultData GetAccessCode(int id)
        {
            var platform = Select.Where(c => c.Id == id).First();
            if (platform == null) return ResultData.No("平台不存在!");
            if (!HasOwnership(platform)) return ResultData.No("无权操作此平台!");

            var request = _httpContextAccessor.HttpContext?.Request;
            string baseApiUrl = $"{request?.Scheme}://{request?.Host}";

            var result = new
            {
                platform.Id,
                platform.PlatformName,
                platform.PlatformKey,
                platform.AccessType,
                AccessTypeName = GetAccessTypeName(platform.AccessType),
                Code = GenerateAccessCode(platform, baseApiUrl)
            };

            return ResultData.Ok(result);
        }

        #region Private Helpers

        private string GenerateAccessCode(ChatPlatform platform, string baseApiUrl)
        {
            switch (platform.AccessType)
            {
                case AccessType.JS:
                    return $@"<!-- SB Agent Hub 客服组件 -->
<script>
window.Sb.Apps.AgentHub = {{
  platformKey: '{platform.PlatformKey}',
  baseURL: '{baseApiUrl}'
}};
</script>
<script src=""{baseApiUrl}/agent-widget.js""></script>";

                case AccessType.Vue:
                    return $@"// 1. 安装依赖
npm install @saibotianlong/agent-widget

// 2. 在组件中使用
import {{ CustomerServiceWidget }} from '@saibotianlong/agent-widget'

<CustomerServiceWidget
  platformKey=""{platform.PlatformKey}""
  :config=""{{
    baseURL: '{baseApiUrl}'
  }}""
/>";

                case AccessType.Chat:
                    return $"{baseApiUrl}/chat?platformKey={platform.PlatformKey}";

                default:
                    return "";
            }
        }

        private static string GetAccessTypeName(AccessType accessType)
        {
            return accessType switch
            {
                AccessType.JS => "JS 嵌入",
                AccessType.Vue => "Vue 组件",
                AccessType.Chat => "独立聊天页",
                _ => "未知"
            };
        }

        private string GeneratePlatformKey()
        {
            return $"pk_{Guid.NewGuid():N}";
        }

        private bool HasOwnership(ChatPlatform platform)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            string userType = user.UserType();

            // Admin 有所有权限
            if (userType == "Admin") return true;

            // Owner 只能操作自己的平台
            return platform.OwnerId == user.Id();
        }

        #endregion
    }
}
