using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 客服分配配置服务
    /// </summary>
    [AutoDependencyInjection]
    public class CustomerServiceConfigService : BaseCrudService<CustomerServiceConfig>
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerServiceConfigService(IFreeSql fsql, ILogger<CustomerServiceConfigService> logger,
            IHttpContextAccessor httpContextAccessor) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取当前 Owner 的配置（不存在则创建默认配置）
        /// </summary>
        public ResultData GetConfig()
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId <= 0) return ResultData.No("授权失效!");

            var config = GetOrCreateConfig(ownerId);
            return ResultData.Ok(config);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public ResultData SaveConfig(CustomerServiceConfig config)
        {
            int ownerId = GetCurrentOwnerId();
            if (ownerId <= 0) return ResultData.No("授权失效!");

            var existing = Select.Where(c => c.OwnerId == ownerId).First();

            if (existing == null)
            {
                config.OwnerId = ownerId;
                config.CreateTime = DateTime.Now;
                config = Insert(config);
                return ResultData.Ok("配置保存成功!", config);
            }

            existing.AllocationRule = config.AllocationRule;
            existing.OfflineHandling = config.OfflineHandling;
            existing.OfflineAutoReply = config.OfflineAutoReply;
            existing.WorkTimeStart = config.WorkTimeStart;
            existing.WorkTimeEnd = config.WorkTimeEnd;
            existing.EnableWorkTime = config.EnableWorkTime;
            existing.NonWorkTimeMessage = config.NonWorkTimeMessage;
            existing.EnableBroadcast = config.EnableBroadcast;
            existing.EnableQueue = config.EnableQueue;
            existing.MaxConcurrentSessions = config.MaxConcurrentSessions;

            UpdateDiy.UpdateColumns(c => new
            {
                c.AllocationRule, c.OfflineHandling, c.OfflineAutoReply,
                c.WorkTimeStart, c.WorkTimeEnd, c.EnableWorkTime, c.NonWorkTimeMessage,
                c.EnableBroadcast, c.EnableQueue, c.MaxConcurrentSessions
            }).SetSource(existing).ExecuteAffrows();

            return ResultData.Ok("配置保存成功!");
        }

        /// <summary>
        /// 获取指定 Owner 的有效配置
        /// </summary>
        public CustomerServiceConfig GetEffectiveConfig(int ownerId)
        {
            return GetOrCreateConfig(ownerId);
        }

        #region Private Helpers

        private CustomerServiceConfig GetOrCreateConfig(int ownerId)
        {
            var config = Select.Where(c => c.OwnerId == ownerId).First();
            if (config != null) return config;

            // 创建默认配置
            config = new CustomerServiceConfig
            {
                OwnerId = ownerId,
                AllocationRule = AllocationRule.RoundRobin,
                OfflineHandling = OfflineHandling.AutoReply,
                OfflineAutoReply = "客服暂时不在线，请稍后再试。",
                EnableWorkTime = false,
                WorkTimeStart = "09:00",
                WorkTimeEnd = "18:00",
                NonWorkTimeMessage = "当前为非工作时间，我们将在工作时间尽快回复您。",
                EnableQueue = false,
                MaxConcurrentSessions = 50,
                CreateTime = DateTime.Now
            };

            config = Insert(config);
            return config;
        }

        private int GetCurrentOwnerId()
        {
            var user = _httpContextAccessor.HttpContext.User;
            string userType = user.UserType();
            string role = user.Role();

            // 超级管理员：返回第一个 Owner 的 Id（管理全局配置）
            if (userType == "Admin")
            {
                var firstOwner = _fsql.Select<Member>().Where(c => c.Role == MemberRole.Owner && c.IsDeleted == false).OrderBy(c => c.Id).First();
                return firstOwner?.Id ?? 0;
            }

            if (userType == "Member" && role == "Owner")
                return user.Id();

            // Agent 返回其所属 Owner
            if (userType == "Member" && role == "Agent")
            {
                int agentId = user.Id();
                var member = _fsql.Select<Member>().Where(c => c.Id == agentId).First();
                return member?.OwnerId ?? 0;
            }

            return 0;
        }

        #endregion
    }
}
