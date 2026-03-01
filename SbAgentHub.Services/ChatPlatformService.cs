using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 接入平台管理服务
    /// </summary>
    [AutoDependencyInjection]
    public class ChatPlatformService : BaseCrudService<ChatPlatform>
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatPlatformService(IFreeSql fsql, ILogger<ChatPlatformService> logger,
            IHttpContextAccessor httpContextAccessor) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 平台列表（Admin 看全部，Owner 看自己的）
        /// </summary>
        public ResultData GetList(string keyword = "", int? ownerId = null, PlatformType? platformType = null,
            bool? isEnabled = null, string field = "", string order = "",
            int page = 1, int pageSize = 20)
        {
            var user = _httpContextAccessor.HttpContext.User;
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
            query = query.WhereIf(ownerId.HasValue, c => c.OwnerId == ownerId.Value);
            query = query.WhereIf(platformType.HasValue, c => c.PlatformType == platformType.Value);
            query = query.WhereIf(isEnabled.HasValue, c => c.IsEnabled == isEnabled.Value);

            query = ApplySorting(query, field, order);

            var platforms = query.Count(out long dataCount).Page(page, pageSize).ToList(c => new
            {
                c.Id, c.OwnerId, c.PlatformName, c.PlatformType, c.PlatformKey,
                c.Domain, c.IsEnabled, c.WelcomeMessage, c.CreateTime,
                c.IsAdminTakeover, c.TakeoverAdminId
            });

            // 批量查询关联名称
            var ownerIds = platforms.Where(p => p.OwnerId > 0).Select(p => p.OwnerId).Distinct().ToList();
            var adminIds = platforms.Where(p => p.TakeoverAdminId.HasValue).Select(p => p.TakeoverAdminId.Value).Distinct().ToList();

            var ownerNames = ownerIds.Any()
                ? _fsql.Select<Member>().Where(m => ownerIds.Contains(m.Id)).ToDictionary(m => m.Id, m => m.Name ?? m.Username)
                : new Dictionary<int, string>();
            var adminNames = adminIds.Any()
                ? _fsql.Select<Admin>().Where(a => adminIds.Contains(a.Id)).ToDictionary(a => a.Id, a => a.Name ?? a.Username)
                : new Dictionary<int, string>();

            var data = platforms.Select(p => new
            {
                p.Id, p.OwnerId, p.PlatformName, p.PlatformType, p.PlatformKey,
                p.Domain, p.IsEnabled, p.WelcomeMessage, p.CreateTime,
                p.IsAdminTakeover, p.TakeoverAdminId,
                OwnerName = p.OwnerId > 0 && ownerNames.ContainsKey(p.OwnerId) ? ownerNames[p.OwnerId] : "",
                TakeoverAdminName = p.TakeoverAdminId.HasValue && adminNames.ContainsKey(p.TakeoverAdminId.Value)
                    ? adminNames[p.TakeoverAdminId.Value] : ""
            });

            return ResultPage.Ok(data, dataCount, page, pageSize, "数据查询成功");
        }

        /// <summary>
        /// 创建接入平台
        /// </summary>
        public ResultData Create(ChatPlatform platform)
        {
            var user = _httpContextAccessor.HttpContext.User;
            string userType = user.UserType();

            if (string.IsNullOrEmpty(platform.PlatformName))
                return ResultData.No("平台名称不能为空!");

            // Owner 创建时自动设置 OwnerId，Admin 创建时使用传入的 OwnerId
            if (userType == "Member")
            {
                platform.OwnerId = user.Id();
            }
            // Admin 创建时：OwnerId 由前端传入（可为 0 表示暂不分配）

            // 自动生成 PlatformKey
            platform.PlatformKey = GeneratePlatformKey();
            platform.CreateTime = DateTime.Now;
            platform.IsDeleted = false;

            platform = Insert(platform);

            return ResultData.Ok("平台创建成功!", new
            {
                platform.Id, platform.PlatformName, platform.PlatformKey,
                platform.PlatformType, platform.OwnerId
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

            var user = _httpContextAccessor.HttpContext.User;
            string userType = user.UserType();

            existing.PlatformName = platform.PlatformName;
            existing.PlatformType = platform.PlatformType;
            existing.Domain = platform.Domain;
            existing.WelcomeMessage = platform.WelcomeMessage;

            if (userType == "Admin")
            {
                existing.OwnerId = platform.OwnerId;
                existing.IsAdminTakeover = platform.IsAdminTakeover;
                existing.TakeoverAdminId = platform.TakeoverAdminId;
            }

            UpdateDiy.UpdateColumns(c => new
            {
                c.PlatformName, c.PlatformType, c.Domain, c.WelcomeMessage,
                c.OwnerId, c.IsAdminTakeover, c.TakeoverAdminId
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
            int userId = _httpContextAccessor.HttpContext.User.Id();

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
        /// 获取站长选项列表
        /// </summary>
        public ResultData GetOwnerOptions()
        {
            var owners = _fsql.Select<Member>()
                .Where(m => m.Role == MemberRole.Owner && m.Status == UserStatus.Active)
                .ToList(m => new { m.Id, m.Name, m.Username });
            return ResultData.Ok(owners);
        }

        /// <summary>
        /// 获取管理员选项列表
        /// </summary>
        public ResultData GetAdminOptions()
        {
            var admins = _fsql.Select<Admin>()
                .Where(a => a.Status == AdminStatus.Active)
                .ToList(a => new { a.Id, a.Name, a.Username });
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

        #region Private Helpers

        private string GeneratePlatformKey()
        {
            return $"pk_{Guid.NewGuid():N}";
        }

        private bool HasOwnership(ChatPlatform platform)
        {
            var user = _httpContextAccessor.HttpContext.User;
            string userType = user.UserType();

            // Admin 有所有权限
            if (userType == "Admin") return true;

            // Owner 只能操作自己的平台
            return platform.OwnerId == user.Id();
        }

        #endregion
    }
}
