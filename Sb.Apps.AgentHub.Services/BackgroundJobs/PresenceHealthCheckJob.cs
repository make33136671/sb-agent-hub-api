using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Models.Dto;
using Sb.Framework.Chat.Realtime.Conversations;
using Sb.Framework.Chat.Realtime.Messages;
using Sb.Framework.Chat.Realtime.Presence;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sb.Apps.AgentHub.Services.BackgroundJobs
{
    /// <summary>
    /// 在线状态健康检查 — 定时扫描数据库中标记为在线的访客，
    /// 与 PresenceService 中的实际连接状态对比，
    /// 发现已断开但未及时清理的，强制标记离线并发送通知。
    /// 作为 SignalR OnDisconnectedAsync 的兜底机制（应对网络异常断开等极端情况）。
    /// </summary>
    public class PresenceHealthCheckJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PresenceHealthCheckJob> _logger;

        /// <summary>检查间隔（默认 30 秒）。</summary>
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

        public PresenceHealthCheckJob(
            IServiceScopeFactory scopeFactory,
            ILogger<PresenceHealthCheckJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PresenceHealthCheckJob 已启动，检查间隔: {Interval}s", CheckInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(CheckInterval, stoppingToken);
                    await CheckAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // 正常停止
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PresenceHealthCheckJob 执行异常");
                }
            }

            _logger.LogInformation("PresenceHealthCheckJob 已停止");
        }

        private async Task CheckAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("PresenceHealthCheck 开始执行...");
            using var scope = _scopeFactory.CreateScope();
            var fsql = scope.ServiceProvider.GetRequiredService<IFreeSql>();
            var presence = scope.ServiceProvider.GetRequiredService<IPresenceService>();
            var conversations = scope.ServiceProvider.GetRequiredService<IConversationService>();
            var messages = scope.ServiceProvider.GetRequiredService<IMessageService>();

            // 0. 清理 PresenceService 中心跳超时的过期条目
            var cleanedUp = presence.CleanupStale();
            if (cleanedUp.Count > 0)
            {
                _logger.LogInformation("PresenceHealthCheck: 清理过期心跳条目 {Count} 个: {UserIds}",
                    cleanedUp.Count, string.Join(", ", cleanedUp));
            }

            // 1. 从数据库获取所有标记为在线的访客
            var onlineVisitors = await fsql.Select<ChatUser>()
                .Where(u => u.OnlineStatus == OnlineStatus.Online && !u.IsDeleted)
                .ToListAsync(stoppingToken);

            _logger.LogDebug("PresenceHealthCheck: 数据库在线访客 {Count} 人", onlineVisitors.Count);

            if (onlineVisitors.Count == 0)
            {
                _logger.LogDebug("PresenceHealthCheck 完成: 无在线访客");
                return;
            }

            var chatUserIds = onlineVisitors.Select(v => v.ChatUserId).ToList();

            // 2. 批量检查实际在线状态
            var presenceStatus = presence.IsOnline(chatUserIds);

            var offlineCount = 0;

            foreach (var visitor in onlineVisitors)
            {
                if (stoppingToken.IsCancellationRequested) break;

                var isActuallyOnline = presenceStatus.TryGetValue(visitor.ChatUserId, out var online) && online;

                if (!isActuallyOnline)
                {
                    // 数据库标记为在线，但 PresenceService 中已无连接 → 强制下线
                    offlineCount++;
                    _logger.LogWarning(
                        "检测到访客在线状态不一致，强制下线: ChatUserId={ChatUserId}, Nickname={Nickname}",
                        visitor.ChatUserId, visitor.Nickname);

                    // 更新数据库
                    await fsql.Update<ChatUser>()
                        .Set(u => u.OnlineStatus == OnlineStatus.Offline)
                        .Where(u => u.Id == visitor.Id)
                        .ExecuteAffrowsAsync(stoppingToken);

                    // 发送下线通知（仅当存在会话时）
                    var convList = await conversations.GetConversationsAsync(visitor.ChatUserId);
                    if (convList.Count > 0)
                    {
                        var payload = JsonSerializer.Serialize(new
                        {
                            type = SystemMessageTypes.VisitorOffline,
                            visitorId = visitor.ChatUserId,
                            reason = "health_check_timeout",
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        });

                        foreach (var conv in convList)
                        {
                            try
                            {
                                await messages.SendSystemAsync(
                                    conv.Id,
                                    SystemMessageType.Prefix + SystemMessageTypes.VisitorOffline,
                                    payload,
                                    visitor.ChatUserId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "发送健康检查下线通知失败: ConversationId={ConvId}", conv.Id);
                            }
                        }
                    }
                }
            }

            if (offlineCount > 0)
            {
                _logger.LogInformation("PresenceHealthCheck 完成: 数据库在线 {DbOnline} 人，强制下线 {Offline} 人",
                    onlineVisitors.Count, offlineCount);
            }
        }
    }
}
