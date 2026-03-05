using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SbAgentHub.Services
{
    /// <summary>
    /// 配额管理服务
    /// </summary>
    [AutoDependencyInjection]
    public class QuotaService : BaseCrudService<QuotaConfig>
    {
        private readonly IFreeSql _fsql;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuotaService(IFreeSql fsql, ILogger<QuotaService> logger,
            IHttpContextAccessor httpContextAccessor) : base(fsql, logger)
        {
            _fsql = fsql;
            _httpContextAccessor = httpContextAccessor;
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
            var configs = _fsql.Select<QuotaConfig>()
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
            var config = _fsql.Select<QuotaConfig>()
                .Where(c => c.QuotaKey == quotaKey && c.IsEnabled)
                .First();
            if (config == null)
                return ResultData.No("配额类型不存在或未启用!");

            // 验证值范围
            if (quotaValue < config.MinValue || quotaValue > config.MaxValue)
                return ResultData.No($"配额值必须在 {config.MinValue} ~ {config.MaxValue} 之间!");

            // 验证站长存在
            var member = _fsql.Select<Member>()
                .Where(m => m.Id == memberId && m.Role == MemberRole.Owner)
                .First();
            if (member == null)
                return ResultData.No("站长不存在!");

            int grantedBy = _httpContextAccessor.HttpContext.User.Id();

            // 查找已有记录
            var existing = _fsql.Select<MemberQuota>()
                .Where(q => q.MemberId == memberId && q.QuotaKey == quotaKey)
                .First();

            if (existing != null)
            {
                existing.QuotaValue = quotaValue;
                existing.GrantedBy = grantedBy;
                existing.GrantedByType = "Admin";
                existing.Remark = remark;
                existing.UpdateTime = DateTime.Now;

                _fsql.Update<MemberQuota>().SetSource(existing)
                    .UpdateColumns(c => new { c.QuotaValue, c.GrantedBy, c.GrantedByType, c.Remark, c.UpdateTime })
                    .ExecuteAffrows();
            }
            else
            {
                _fsql.Insert(new MemberQuota
                {
                    MemberId = memberId,
                    QuotaKey = quotaKey,
                    QuotaValue = quotaValue,
                    GrantedBy = grantedBy,
                    GrantedByType = "Admin",
                    Remark = remark,
                    CreateTime = DateTime.Now,
                    IsDeleted = false
                }).ExecuteAffrows();
            }

            return ResultData.Ok("配额设置成功!");
        }

        /// <summary>
        /// 批量设置配额
        /// </summary>
        public ResultData BatchSetMemberQuotas(int memberId, List<QuotaItem> quotas, string remark = "")
        {
            if (quotas == null || !quotas.Any())
                return ResultData.No("配额列表不能为空!");

            // 验证站长存在
            var member = _fsql.Select<Member>()
                .Where(m => m.Id == memberId && m.Role == MemberRole.Owner)
                .First();
            if (member == null)
                return ResultData.No("站长不存在!");

            int successCount = 0;
            var errors = new List<string>();

            foreach (var item in quotas)
            {
                var result = SetMemberQuota(memberId, item.QuotaKey, item.QuotaValue, remark);
                if (result.Code == 0)
                    successCount++;
                else
                    errors.Add($"{item.QuotaKey}: {result.Message}");
            }

            if (errors.Any())
                return ResultData.No($"部分配额设置失败: {string.Join("; ", errors)}");

            return ResultData.Ok($"成功设置 {successCount} 项配额!");
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
            var configs = _fsql.Select<QuotaConfig>()
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
            var owners = _fsql.Select<Member>()
                .Where(m => m.Role == MemberRole.Owner && m.Status == UserStatus.Active)
                .ToList(m => new { m.Id, m.Name, m.Username });
            return ResultData.Ok(owners);
        }

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
            var config = _fsql.Select<QuotaConfig>()
                .Where(c => c.QuotaKey == quotaKey)
                .First();
            return config?.DefaultValue ?? 0;
        }

        /// <summary>
        /// 计算当前使用量
        /// </summary>
        private int GetCurrentUsage(int memberId, string quotaKey)
        {
            switch (quotaKey)
            {
                case "max_platforms":
                    return (int)_fsql.Select<ChatPlatform>()
                        .Where(p => p.OwnerId == memberId)
                        .Count();

                case "max_agents":
                    return (int)_fsql.Select<Member>()
                        .Where(m => m.OwnerId == memberId && m.Role == MemberRole.Agent)
                        .Count();

                case "max_concurrent_sessions":
                    // 实时计算：该站长下所有在线访客数
                    return (int)_fsql.Select<ChatUser>()
                        .Where(u => u.OwnerId == memberId && u.OnlineStatus == OnlineStatus.Online)
                        .Count();

                case "max_daily_visitors":
                    // 实时计算：今日该站长所有平台的登录记录数（去重 ChatUserId）
                    var today = DateTime.Today;
                    var tomorrow = today.AddDays(1);
                    var platformIds = _fsql.Select<ChatPlatform>()
                        .Where(p => p.OwnerId == memberId)
                        .ToList(p => p.Id);
                    if (!platformIds.Any()) return 0;
                    return (int)_fsql.Select<ChatUserLoginRecord>()
                        .Where(r => platformIds.Contains(r.PlatformId)
                            && r.LoginTime >= today && r.LoginTime < tomorrow)
                        .Distinct()
                        .Count();

                case "storage_quota_mb":
                    // 实时计算：该站长所有文件的总大小（转换为 MB）
                    var totalBytes = _fsql.Select<FileRecord>()
                        .Where(f => f.OwnerId == memberId)
                        .Sum(f => f.FileSize);
                    return (int)(totalBytes / (1024 * 1024));

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

        #endregion
    }

    /// <summary>
    /// 配额检查结果
    /// </summary>
    public class QuotaCheckResult
    {
        public string QuotaKey { get; set; }
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
        public string QuotaKey { get; set; }
        public int QuotaValue { get; set; }
    }
}
