using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sb.Apps.AgentHub.Services.BackgroundJobs
{
    /// <summary>
    /// 定时任务基类 — 每隔固定间隔执行一次
    /// </summary>
    public abstract class ScheduledJobBase : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private readonly TimeSpan _interval;
        private readonly string _jobName;

        protected ScheduledJobBase(
            IServiceScopeFactory scopeFactory,
            ILogger logger,
            TimeSpan interval,
            string jobName)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _interval = interval;
            _jobName = jobName;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("定时任务 [{JobName}] 已启动，间隔 {Interval}", _jobName, _interval);

            // 启动后延迟 30 秒，避免启动时大量 IO
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    await ExecuteJobAsync(scope.ServiceProvider, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "定时任务 [{JobName}] 执行异常", _jobName);
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("定时任务 [{JobName}] 已停止", _jobName);
        }

        protected abstract Task ExecuteJobAsync(IServiceProvider services, CancellationToken ct);
    }
}
