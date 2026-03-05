using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbAgentHub.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbAgentHub.Services.BackgroundJobs
{
    /// <summary>
    /// 消息保留清理定时任务 — 每天执行一次，按站长的 message_retention_days 配额清理过期消息
    /// </summary>
    public class MessageCleanupJob : ScheduledJobBase
    {
        public MessageCleanupJob(
            IServiceScopeFactory scopeFactory,
            ILogger<MessageCleanupJob> logger)
            : base(scopeFactory, logger, TimeSpan.FromHours(24), "MessageCleanup")
        {
        }

        protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken ct)
        {
            var fsql = services.GetRequiredService<IFreeSql>();
            var quotaService = services.GetRequiredService<QuotaService>();
            var logger = services.GetRequiredService<ILogger<MessageCleanupJob>>();

            // 获取所有站长
            var owners = fsql.Select<Member>()
                .Where(m => m.Role == MemberRole.Owner && m.Status == UserStatus.Active)
                .ToList(m => m.Id);

            int totalCleaned = 0;

            foreach (var ownerId in owners)
            {
                if (ct.IsCancellationRequested) break;

                int retentionDays = quotaService.GetEffectiveQuotaValue(ownerId, "message_retention_days");
                if (retentionDays <= 0) continue; // 0 表示永久保留

                var cutoff = DateTime.Now.AddDays(-retentionDays);

                // 清理该站长的聊天用户登录记录
                var cleaned = fsql.Delete<ChatUserLoginRecord>()
                    .Where(r => r.LoginTime < cutoff)
                    .Where(r => fsql.Select<ChatUser>()
                        .Where(u => u.Id == r.ChatUserId && u.OwnerId == ownerId)
                        .Any())
                    .ExecuteAffrows();

                if (cleaned > 0)
                {
                    totalCleaned += cleaned;
                    logger.LogInformation("清理过期记录: OwnerId={OwnerId}, RetentionDays={Days}, Cleaned={Count}",
                        ownerId, retentionDays, cleaned);
                }
            }

            if (totalCleaned > 0)
            {
                logger.LogInformation("消息清理任务完成: 共清理 {Total} 条过期记录", totalCleaned);
            }

            await Task.CompletedTask;
        }
    }
}
