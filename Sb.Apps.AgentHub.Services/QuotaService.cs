using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 配额管理服务（管理 QuotaConfig / MemberQuota / QuotaUsage 三张配额表）
    /// </summary>
    [AutoDependencyInjection]
    public class QuotaService : BaseCrudService<QuotaConfig>
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MemberService _memberService;
        private readonly ChatUserService _chatUserService;
        private readonly IServiceProvider _serviceProvider;

        // 懒加载打破循环依赖：QuotaService→ChatPlatformService/FileUploadService（两者亦反向注入 QuotaService）
        private readonly Lazy<ChatPlatformService> _platformService;
        private readonly Lazy<FileUploadService> _fileUploadService;

        public QuotaService(IFreeSql fsql, ILogger<QuotaService> logger,
            IHttpContextAccessor httpContextAccessor,
            MemberService memberService,
            ChatUserService chatUserService,
            IServiceProvider serviceProvider) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
            _memberService = memberService;
            _chatUserService = chatUserService;
            _serviceProvider = serviceProvider;
            _platformService = new Lazy<ChatPlatformService>(() => _serviceProvider.GetRequiredService<ChatPlatformService>());
            _fileUploadService = new Lazy<FileUploadService>(() => _serviceProvider.GetRequiredService<FileUploadService>());
        }

        /// <summary>
        /// 获取所有配额定义（启用的）
        /// </summary>
        public ResultData GetQuotaConfigs(bool enabledOnly = true)
        {
            var query = Select;
            if (enabledOnly)
                query = query.Where(c => c.IsEnabled);

            var configs = query.OrderBy(c => c.SortOrder).ToList(c => new
            {
                c.Id,
                c.QuotaKey,
                c.QuotaName,
                c.QuotaType,
                c.DefaultValue,
                c.MinValue,
                c.MaxValue,
                c.Unit,
                c.Description,
                c.SortOrder,
                c.IsEnabled
            });

            return ResultData.Ok(configs);
        }

        /// <summary>
        /// 获取站长的所有配额（含使用量）
        /// </summary>
        public ResultData GetMemberQuotas(int memberId)
        {
            // 获取所有启用的配额定义
            var configs = Select
                .Where(c => c.IsEnabled)
                .OrderBy(c => c.SortOrder)
                .ToList();

            // 获取该站长的已分配配额
            var quotas = _fsql.Select<MemberQuota>()
                .Where(c => c.MemberId == memberId)
                .ToList();

            var quotaDict = quotas.ToDictionary(q => q.QuotaKey, q => q);

            var result = configs.Select(config =>
            {
                var memberQuota = quotaDict.ContainsKey(config.QuotaKey) ? quotaDict[config.QuotaKey] : null;
                int effectiveValue = memberQuota?.QuotaValue ?? config.DefaultValue;
                int currentUsage = GetCurrentUsage(memberId, config.QuotaKey);

                return new
                {
                    config.QuotaKey,
                    config.QuotaName,
                    config.QuotaType,
                    config.DefaultValue,
                    config.MinValue,
                    config.MaxValue,
                    config.Unit,
                    config.Description,
                    QuotaValue = effectiveValue,
                    IsCustomized = memberQuota != null,
                    CurrentUsage = currentUsage,
                    Remaining = effectiveValue - currentUsage,
                    GrantedBy = memberQuota?.GrantedBy,
                    GrantedByType = memberQuota?.GrantedByType,
                    ExpiresAt = memberQuota?.ExpiresAt,
                    Remark = memberQuota?.Remark ?? ""
                };
            }).ToList();

            return ResultData.Ok(result);
        }

        /// <summary>
        /// 设置单个配额
        /// </summary>
        public ResultData SetMemberQuota(int memberId, string quotaKey, int quotaValue, string remark = "")
        {
            // 验证配额定义存在
            var config = Select
                .Where(c => c.QuotaKey == quotaKey && c.IsEnabled)
                .First();
            if (config == null)
                return ResultData.No("配额类型不存在或未启用!");

            // 验证值范围
            if (quotaValue < config.MinValue || quotaValue > config.MaxValue)
                return ResultData.No($"配额值必须在 {config.MinValue} ~ {config.MaxValue} 之间!");

            // 验证站长存在
            var member = _memberService.GetOwner(memberId);
            if (member == null)
                return ResultData.No("站长不存在!");

            int grantedBy = _httpContextAccessor.HttpContext!.User.Id();

            UpsertMemberQuota(memberId, quotaKey, quotaValue, "Admin", null, remark, null, grantedBy);

            return ResultData.Ok("配额设置成功!");
        }

        /// <summary>
        /// 批量设置配额 — 事务包裹全部写入
        /// </summary>
        public ResultData BatchSetMemberQuotas(int memberId, List<QuotaItem> quotas, string remark = "")
        {
            if (quotas == null || !quotas.Any())
                return ResultData.No("配额列表不能为空!");

            var member = _memberService.GetOwner(memberId);
            if (member == null)
                return ResultData.No("站长不存在!");

            int grantedBy = _httpContextAccessor.HttpContext!.User.Id();

            using var uow = _fsql.CreateUnitOfWork();
            var tx = uow.GetOrBeginTransaction();
            try
            {
                foreach (var item in quotas)
                {
                    var config = Select
                        .Where(c => c.QuotaKey == item.QuotaKey && c.IsEnabled)
                        .First();
                    if (config == null)
                        throw new Exception($"配额类型 {item.QuotaKey} 不存在或未启用");

                    if (item.QuotaValue < config.MinValue || item.QuotaValue > config.MaxValue)
                        throw new Exception($"配额 {item.QuotaKey} 值必须在 {config.MinValue} ~ {config.MaxValue} 之间");

                    UpsertMemberQuota(memberId, item.QuotaKey, item.QuotaValue, "Admin", null, remark, tx, grantedBy);
                }

                uow.Commit();
                return ResultData.Ok($"成功设置 {quotas.Count} 项配额!");
            }
            catch (Exception ex)
            {
                uow.Rollback();
                return ResultData.No($"批量配额设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查配额是否充足
        /// </summary>
        public QuotaCheckResult CheckQuota(int memberId, string quotaKey)
        {
            int effectiveQuota = GetEffectiveQuota(memberId, quotaKey);
            int currentUsage = GetCurrentUsage(memberId, quotaKey);

            return new QuotaCheckResult
            {
                QuotaKey = quotaKey,
                Limit = effectiveQuota,
                Used = currentUsage,
                Remaining = effectiveQuota - currentUsage,
                IsAvailable = currentUsage < effectiveQuota
            };
        }

        /// <summary>
        /// 获取配额使用概览
        /// </summary>
        public ResultData GetUsageSummary(int memberId)
        {
            var configs = Select
                .Where(c => c.IsEnabled)
                .OrderBy(c => c.SortOrder)
                .ToList();

            var result = configs.Select(config =>
            {
                int effectiveQuota = GetEffectiveQuota(memberId, config.QuotaKey);
                int currentUsage = GetCurrentUsage(memberId, config.QuotaKey);

                return new
                {
                    config.QuotaKey,
                    config.QuotaName,
                    config.QuotaType,
                    config.Unit,
                    Limit = effectiveQuota,
                    Used = currentUsage,
                    Remaining = effectiveQuota - currentUsage,
                    UsagePercent = effectiveQuota > 0 ? Math.Round((double)currentUsage / effectiveQuota * 100, 1) : 0
                };
            }).ToList();

            return ResultData.Ok(result);
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

        #region 仓储访问入口（供其他 Service 调用）

        /// <summary>
        /// 按主键获取配额定义
        /// </summary>
        public QuotaConfig GetById(int id)
        {
            return Select.Where(c => c.Id == id).First();
        }

        /// <summary>
        /// 按 QuotaKey 获取启用的配额定义
        /// </summary>
        public QuotaConfig GetEnabledByQuotaKey(string quotaKey)
        {
            return Select.Where(c => c.QuotaKey == quotaKey && c.IsEnabled).First();
        }

        /// <summary>
        /// 获取所有启用的配额定义
        /// </summary>
        public List<QuotaConfig> GetEnabledConfigs()
        {
            return Select.Where(c => c.IsEnabled).OrderBy(c => c.SortOrder).ToList();
        }

        /// <summary>
        /// 获取启用的数量型配额定义（QuotaType == 0）
        /// </summary>
        public List<QuotaConfig> GetEnabledQuantityConfigs()
        {
            return Select.Where(c => c.IsEnabled && c.QuotaType == 0).ToList();
        }

        /// <summary>
        /// 按 QuotaKey 获取配额定义（不限启用状态）
        /// </summary>
        public QuotaConfig GetByQuotaKey(string quotaKey)
        {
            return Select.Where(c => c.QuotaKey == quotaKey).First();
        }

        /// <summary>
        /// 获取站长的到期提醒配额（7天内到期）
        /// </summary>
        public List<MemberQuota> GetExpiringQuotas(int memberId, DateTime now, int daysAhead = 7)
        {
            return _fsql.Select<MemberQuota>()
                .Where(q => q.MemberId == memberId
                    && q.ExpiresAt != null
                    && q.ExpiresAt > now
                    && q.ExpiresAt <= now.AddDays(daysAhead))
                .ToList();
        }

        /// <summary>
        /// 获取站长的有效配额值（公开方法，供其他服务调用）
        /// </summary>
        public int GetEffectiveQuotaValue(int memberId, string quotaKey)
        {
            return GetEffectiveQuota(memberId, quotaKey);
        }

        /// <summary>
        /// 检查功能开关是否启用（开关型配额，值 >= 1 视为启用）
        /// </summary>
        public bool CheckFeature(int memberId, string featureKey)
        {
            return GetEffectiveQuota(memberId, featureKey) >= 1;
        }

        /// <summary>
        /// 插入或更新站长配额（供 MemberService/OrderService 在事务内调用）
        /// </summary>
        public void UpsertMemberQuota(int memberId, string quotaKey, int quotaValue,
            string grantedByType, DateTime? expiresAt, string remark,
            DbTransaction? tx = null, int? grantedBy = null)
        {
            var existing = _fsql.Select<MemberQuota>()
                .Where(q => q.MemberId == memberId && q.QuotaKey == quotaKey)
                .First();

            if (existing != null)
            {
                existing.QuotaValue = quotaValue;
                if (grantedBy.HasValue) existing.GrantedBy = grantedBy;
                existing.GrantedByType = grantedByType;
                existing.ExpiresAt = expiresAt;
                existing.Remark = remark;
                existing.UpdateTime = DateTime.Now;

                _fsql.Update<MemberQuota>().SetSource(existing)
                    .UpdateColumns(q => new
                    {
                        q.QuotaValue, q.GrantedBy, q.GrantedByType,
                        q.ExpiresAt, q.Remark, q.UpdateTime
                    }).WithTransaction(tx).ExecuteAffrows();
            }
            else
            {
                _fsql.Insert(new MemberQuota
                {
                    MemberId = memberId,
                    QuotaKey = quotaKey,
                    QuotaValue = quotaValue,
                    GrantedBy = grantedBy,
                    GrantedByType = grantedByType,
                    ExpiresAt = expiresAt,
                    Remark = remark,
                    CreateTime = DateTime.Now,
                    IsDeleted = false
                }).WithTransaction(tx).ExecuteAffrows();
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// 获取有效配额值（优先 MemberQuota，fallback QuotaConfig.DefaultValue）
        /// </summary>
        private int GetEffectiveQuota(int memberId, string quotaKey)
        {
            var memberQuota = _fsql.Select<MemberQuota>()
                .Where(q => q.MemberId == memberId && q.QuotaKey == quotaKey)
                .First();

            if (memberQuota != null)
            {
                // 检查是否过期
                if (memberQuota.ExpiresAt.HasValue && memberQuota.ExpiresAt.Value < DateTime.Now)
                {
                    // 过期了，回退到默认值
                    return GetDefaultQuota(quotaKey);
                }
                return memberQuota.QuotaValue;
            }

            return GetDefaultQuota(quotaKey);
        }

        /// <summary>
        /// 获取配额默认值
        /// </summary>
        private int GetDefaultQuota(string quotaKey)
        {
            var config = Select
                .Where(c => c.QuotaKey == quotaKey)
                .First();
            return config?.DefaultValue ?? 0;
        }

        /// <summary>
        /// 计算当前使用量 — 通过对应 Service 查询，避免跨实体 _fsql 调用
        /// </summary>
        private int GetCurrentUsage(int memberId, string quotaKey)
        {
            switch (quotaKey)
            {
                case "max_platforms":
                    return _platformService.Value.CountByOwner(memberId);

                case "max_agents":
                    return _memberService.CountActiveAgentsByOwner(memberId);

                case "max_concurrent_sessions":
                    return _chatUserService.CountOnlineByOwner(memberId);

                case "max_daily_visitors":
                    var today = DateTime.Today;
                    var tomorrow = today.AddDays(1);
                    var platformIds = _platformService.Value.GetPlatformIdsByOwner(memberId);
                    if (!platformIds.Any()) return 0;
                    return _chatUserService.CountDailyLoginsByPlatformIds(platformIds, today, tomorrow);

                case "storage_quota_mb":
                    return (int)_fileUploadService.Value.GetStorageUsageMB(memberId);

                case "message_retention_days":
                    // 这是配置值，不是用量，返回 0
                    return 0;

                default:
                    // 其他配额维度从 QuotaUsage 表读取
                    var usage = _fsql.Select<QuotaUsage>()
                        .Where(u => u.MemberId == memberId && u.QuotaKey == quotaKey)
                        .First();
                    return usage?.CurrentUsage ?? 0;
            }
        }

        #endregion

        #region QuotaUsage 更新方法

        /// <summary>
        /// 设置配额用量（绝对值）
        /// </summary>
        public void UpdateUsage(int memberId, string quotaKey, int currentUsage)
        {
            var existing = _fsql.Select<QuotaUsage>()
                .Where(u => u.MemberId == memberId && u.QuotaKey == quotaKey)
                .First();

            if (existing != null)
            {
                existing.CurrentUsage = currentUsage;
                existing.UpdateTime = DateTime.Now;
                _fsql.Update<QuotaUsage>().SetSource(existing)
                    .UpdateColumns(u => new { u.CurrentUsage, u.UpdateTime })
                    .ExecuteAffrows();
            }
            else
            {
                _fsql.Insert(new QuotaUsage
                {
                    MemberId = memberId,
                    QuotaKey = quotaKey,
                    CurrentUsage = currentUsage,
                    UpdateTime = DateTime.Now,
                    IsDeleted = false
                }).ExecuteAffrows();
            }
        }

        /// <summary>
        /// 增减配额用量（delta 可为负数）
        /// </summary>
        public void IncrementUsage(int memberId, string quotaKey, int delta)
        {
            var existing = _fsql.Select<QuotaUsage>()
                .Where(u => u.MemberId == memberId && u.QuotaKey == quotaKey)
                .First();

            if (existing != null)
            {
                existing.CurrentUsage = Math.Max(0, existing.CurrentUsage + delta);
                existing.UpdateTime = DateTime.Now;
                _fsql.Update<QuotaUsage>().SetSource(existing)
                    .UpdateColumns(u => new { u.CurrentUsage, u.UpdateTime })
                    .ExecuteAffrows();
            }
            else if (delta > 0)
            {
                _fsql.Insert(new QuotaUsage
                {
                    MemberId = memberId,
                    QuotaKey = quotaKey,
                    CurrentUsage = delta,
                    UpdateTime = DateTime.Now,
                    IsDeleted = false
                }).ExecuteAffrows();
            }
        }

        #endregion
    }

    /// <summary>
    /// 配额检查结果
    /// </summary>
    public class QuotaCheckResult
    {
        public string QuotaKey { get; set; } = null!;
        public int Limit { get; set; }
        public int Used { get; set; }
        public int Remaining { get; set; }
        public bool IsAvailable { get; set; }
    }

    /// <summary>
    /// 批量设置配额项
    /// </summary>
    public class QuotaItem
    {
        public string QuotaKey { get; set; } = null!;
        public int QuotaValue { get; set; }
    }
}
