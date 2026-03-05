using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SbAgentHub.Services.BackgroundJobs
{
    /// <summary>
    /// 排队超时清理定时任务 — 每 5 分钟清理超过 30 分钟的排队记录
    /// </summary>
    public class QueueCleanupJob : ScheduledJobBase
    {
        public QueueCleanupJob(
            IServiceScopeFactory scopeFactory,
            ILogger<QueueCleanupJob> logger)
            : base(scopeFactory, logger, TimeSpan.FromMinutes(5), "QueueCleanup")
        {
        }

        protected override async Task ExecuteJobAsync(IServiceProvider services, CancellationToken ct)
        {
            var queueService = services.GetRequiredService<QueueService>();
            var logger = services.GetRequiredService<ILogger<QueueCleanupJob>>();

            int cleaned = queueService.CleanupTimeout(30);
            if (cleaned > 0)
            {
                logger.LogInformation("清理超时排队记录: {Count} 条", cleaned);
            }

            await Task.CompletedTask;
        }
    }
}
