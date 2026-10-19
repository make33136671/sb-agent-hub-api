using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sb.Apps.AgentHub.Services.BackgroundJobs
{
    /// <summary>
    /// 配额过期处理定时任务 — 每小时执行一次
    /// 1. 检查即将过期的配额（7天/3天/1天内），记录预警
    /// 2. 处理已过期的配额，降级为默认值
    /// </summary>
    public class QuotaExpirationJob : ScheduledJobBase
    {
        public QuotaExpirationJob(
            IServiceScopeFactory scopeFactory,
            ILogger<QuotaExpirationJob> logger)
            : base(scopeFactory, logger, TimeSpan.FromHours(1), "QuotaExpiration")
        {
        }

        protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken ct)
        {
            var fsql = services.GetRequiredService<IFreeSql>();
            var logger = services.GetRequiredService<ILogger<QuotaExpirationJob>>();

            var now = DateTime.Now;

            // 1. 处理已过期的配额 — 降级为默认值
            var expiredQuotas = fsql.Select<MemberQuota>()
                .Where(q => q.ExpiresAt != null && q.ExpiresAt < now)
                .ToList();

            if (expiredQuotas.Any())
            {
                foreach (var quota in expiredQuotas)
                {
                    if (ct.IsCancellationRequested) break;

                    // 获取默认值
                    var config = fsql.Select<QuotaConfig>()
                        .Where(c => c.QuotaKey == quota.QuotaKey)
                        .First();
                    int defaultValue = config?.DefaultValue ?? 0;

                    // 降级为默认值
                    quota.QuotaValue = defaultValue;
                    quota.ExpiresAt = null;
                    quota.GrantedByType = "System";
                    quota.Remark = $"套餐过期，自动降级为默认值({defaultValue})";
                    quota.UpdateTime = now;

                    fsql.Update<MemberQuota>().SetSource(quota)
                        .UpdateColumns(q => new
                        {
                            q.QuotaValue, q.ExpiresAt, q.GrantedByType,
                            q.Remark, q.UpdateTime
                        }).ExecuteAffrows();

                    logger.LogInformation("配额过期降级: MemberId={MemberId} QuotaKey={Key} -> DefaultValue={Value}",
                        quota.MemberId, quota.QuotaKey, defaultValue);
                }

                logger.LogInformation("配额过期处理完成: 共处理 {Count} 条过期配额", expiredQuotas.Count);
            }

            // 2. 检查即将过期的配额 — 记录预警日志
            var warningDays = new[] { 7, 3, 1 };
            foreach (var days in warningDays)
            {
                if (ct.IsCancellationRequested) break;

                var deadline = now.AddDays(days);
                var previousDeadline = now.AddDays(days + 1); // 避免重复提醒

                var soonExpiring = fsql.Select<MemberQuota>()
                    .Where(q => q.ExpiresAt != null
                        && q.ExpiresAt > now
                        && q.ExpiresAt <= deadline
                        && q.ExpiresAt > previousDeadline.AddHours(-1))
                    .ToList();

                foreach (var quota in soonExpiring)
                {
                    var member = fsql.Select<Member>()
                        .Where(m => m.Id == quota.MemberId)
                        .First();

                    logger.LogWarning("配额即将过期提醒: MemberId={MemberId} Member={Name} QuotaKey={Key} ExpiresAt={Expires} ({Days}天内)",
                        quota.MemberId, member?.Name ?? "unknown", quota.QuotaKey, quota.ExpiresAt, days);

                    // 记录到系统通知表（如果有的话），或发送邮件/站内信
                    // 当前先记录日志，后续可扩展通知渠道
                }
            }

            // 3. 检查超额预警
            await CheckOverageWarnings(fsql, logger, ct);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 检查配额超额预警
        /// </summary>
        private async Task CheckOverageWarnings(IFreeSql fsql, ILogger logger, CancellationToken ct)
        {
            var owners = fsql.Select<Member>()
                .Where(m => m.Role == MemberRole.Owner && m.Status == UserStatus.Active)
                .ToList(m => m.Id);

            // 只检查数量型配额（QuotaType=0），不含开关型（1）和存储型（2 暂不检查）
            var quotaConfigs = fsql.Select<QuotaConfig>()
                .Where(c => c.IsEnabled && c.QuotaType == 0)
                .ToList();

            foreach (var ownerId in owners)
            {
                if (ct.IsCancellationRequested) break;

                foreach (var config in quotaConfigs)
                {
                    var memberQuota = fsql.Select<MemberQuota>()
                        .Where(q => q.MemberId == ownerId && q.QuotaKey == config.QuotaKey)
                        .First();
                    int limit = memberQuota?.QuotaValue ?? config.DefaultValue;
                    if (limit <= 0) continue;

                    // 简化用量计算（仅处理核心配额）
                    int used = 0;
                    switch (config.QuotaKey)
                    {
                        case "max_platforms":
                            used = (int)fsql.Select<ChatPlatform>()
                                .Where(p => p.OwnerId == ownerId).Count();
                            break;
                        case "max_agents":
                            used = (int)fsql.Select<Member>()
                                .Where(m => m.OwnerId == ownerId && m.Role == MemberRole.Agent).Count();
                            break;
                        default:
                            continue;
                    }

                    double usagePercent = (double)used / limit * 100;

                    if (usagePercent >= 95)
                    {
                        logger.LogError("配额超额预警[红色]: OwnerId={OwnerId} {Key} 使用率={Percent:F1}% ({Used}/{Limit})",
                            ownerId, config.QuotaKey, usagePercent, used, limit);
                    }
                    else if (usagePercent >= 80)
                    {
                        logger.LogWarning("配额超额预警[黄色]: OwnerId={OwnerId} {Key} 使用率={Percent:F1}% ({Used}/{Limit})",
                            ownerId, config.QuotaKey, usagePercent, used, limit);
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}
