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
    /// 访客在线状态处理器 — 监听 PresenceService 的上下线事件。
    /// - 上线：仅更新数据库 OnlineStatus（上线通知由客户端主动发送，避免重复）
    /// - 下线：更新数据库状态 + 向会话发送 visitor_offline 系统消息（客户端关闭时 sendBeacon 不可靠，服务端兜底）
    /// </summary>
    public class VisitorPresenceHandler : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<VisitorPresenceHandler> _logger;
        private IPresenceService? _presence;

        public VisitorPresenceHandler(
            IServiceScopeFactory scopeFactory,
            ILogger<VisitorPresenceHandler> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 在作用域中获取 IPresenceService 并订阅事件
            var scope = _scopeFactory.CreateScope();
            _presence = scope.ServiceProvider.GetRequiredService<IPresenceService>();
            _presence.PresenceChanged += OnPresenceChanged;

            _logger.LogInformation("VisitorPresenceHandler 已启动，监听访客在线状态变化");

            // 等待停止令牌触发时取消订阅
            stoppingToken.Register(() =>
            {
                if (_presence != null)
                {
                    _presence.PresenceChanged -= OnPresenceChanged;
                    scope.Dispose();
                    _logger.LogInformation("VisitorPresenceHandler 已停止");
                }
            });

            return Task.CompletedTask;
        }

        private void OnPresenceChanged(PresenceChange change)
        {
            // 异步处理，避免阻塞 PresenceService 的锁
            _ = HandlePresenceChangeAsync(change.UserId, change.Online);
        }

        private async Task HandlePresenceChangeAsync(string userId, bool online)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var fsql = scope.ServiceProvider.GetRequiredService<IFreeSql>();
                var conversations = scope.ServiceProvider.GetRequiredService<IConversationService>();
                var messages = scope.ServiceProvider.GetRequiredService<IMessageService>();

                // 判断是否为访客（在 ChatUser 表中查找）
                var chatUser = await fsql.Select<ChatUser>()
                    .Where(u => u.ChatUserId == userId && !u.IsDeleted)
                    .FirstAsync();

                if (chatUser == null)
                {
                    // 不是访客（可能是客服），跳过
                    return;
                }

                var statusText = online ? "上线" : "下线";
                _logger.LogDebug("访客{Status}: ChatUserId={ChatUserId}, Nickname={Nickname}",
                    statusText, userId, chatUser.Nickname);

                // 更新访客在线状态
                var targetStatus = online ? OnlineStatus.Online : OnlineStatus.Offline;
                await fsql.Update<ChatUser>()
                    .Set(u => u.OnlineStatus == targetStatus)
                    .Where(u => u.Id == chatUser.Id)
                    .ExecuteAffrowsAsync();

                // 仅下线时发送系统消息（上线通知由客户端主动发送，避免重复）
                if (online) return;

                // 获取该访客的所有会话
                var convList = await conversations.GetConversationsAsync(userId);
                if (convList.Count == 0) return;

                var msgType = SystemMessageTypes.VisitorOffline;

                var payload = JsonSerializer.Serialize(new
                {
                    type = msgType,
                    visitorId = userId,
                    reason = "connection_lost",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });

                foreach (var conv in convList)
                {
                    try
                    {
                        await messages.SendSystemAsync(
                            conv.Id,
                            SystemMessageType.Prefix + msgType,
                            payload,
                            userId);

                        _logger.LogDebug("已发送访客下线通知: ConversationId={ConvId}, VisitorId={VisitorId}",
                            conv.Id, userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "发送访客下线通知失败: ConversationId={ConvId}, VisitorId={VisitorId}",
                            conv.Id, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理访客状态变更事件异常: UserId={UserId}, Online={Online}", userId, online);
            }
        }
    }
}
