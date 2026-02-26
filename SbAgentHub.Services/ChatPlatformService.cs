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

            var data = query.Count(out long dataCount).Page(page, pageSize).ToList(c => new
            {
                c.Id, c.OwnerId, c.PlatformName, c.PlatformType, c.PlatformKey,
                c.Domain, c.IsEnabled, c.WelcomeMessage, c.CreateTime
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

            // Owner 创建时自动设置 OwnerId
            if (userType == "Member")
            {
                platform.OwnerId = user.Id();
            }

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

            existing.PlatformName = platform.PlatformName;
            existing.PlatformType = platform.PlatformType;
            existing.Domain = platform.Domain;
            existing.WelcomeMessage = platform.WelcomeMessage;

            UpdateDiy.UpdateColumns(c => new
            {
                c.PlatformName, c.PlatformType, c.Domain, c.WelcomeMessage
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
