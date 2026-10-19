using Microsoft.Extensions.Logging;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Util;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Models.Dto;
using Sb.Framework.Chat.Realtime.Conversations;
using Sb.Framework.Chat.Realtime.Messages;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 聊天会话编排服务 — 负责 Realtime 会话创建、系统消息发送与客服 Realtime 用户 ID 确保。
    /// 从 ChatAuthService 拆分，聚焦会话编排职责。
    /// </summary>
    [AutoDependencyInjection]
    public class ChatSessionOrchestrator
    {
        private readonly ILogger<ChatSessionOrchestrator> _logger;
        private readonly IConversationService _conversations;
        private readonly IMessageService _messages;
        private readonly IMessageDeliveryService _delivery;
        private readonly CustomerServiceConfigService _configService;
        private readonly MemberService _memberService;
        private readonly ChatUserService _chatUserService;

        public ChatSessionOrchestrator(
            ILogger<ChatSessionOrchestrator> logger,
            IConversationService conversations,
            IMessageService messages,
            IMessageDeliveryService delivery,
            CustomerServiceConfigService configService,
            MemberService memberService,
            ChatUserService chatUserService)
        {
            _logger = logger;
            _conversations = conversations;
            _messages = messages;
            _delivery = delivery;
            _configService = configService;
            _memberService = memberService;
            _chatUserService = chatUserService;
        }

        /// <summary>
        /// 为会话创建 Realtime 会话（统一群聊模式：主客服为创建者，访客为成员；广播模式追加该 owner 下所有在线客服）
        /// </summary>
        public async Task<string> CreateSessionConversationAsync(ChatUser chatUser, AgentAllocationResult allocation, ChatPlatform platform)
        {
            var csConfig = _configService.GetEffectiveConfig(platform.OwnerId);

            // 会话成员：访客
            var memberIds = new List<string> { chatUser.ChatUserId };

            // 广播模式：追加该 owner 下所有在线客服
            if (csConfig.EnableBroadcast)
            {
                var onlineAgents = _memberService.GetOnlineAgentRealtimeUserIds(platform.OwnerId, allocation.AgentId!.Value);
                memberIds.AddRange(onlineAgents);
            }

            var groupName = $"客服会话_{chatUser.Nickname ?? "访客"}";
            var conversation = await _conversations.CreateAsync(
                platform.OwnerId.ToString(),
                "group",
                groupName,
                memberIds,
                allocation.RealtimeUserId);

            var conversationId = conversation.Id.ToString();
            if (!string.IsNullOrEmpty(conversationId))
            {
                chatUser.GroupId = conversationId;
                _chatUserService.UpdateColumns(chatUser, c => c.GroupId);

                // 通知客服有新会话（推送到 agent:{userId} 个人组，前端据此刷新会话列表并加入 SignalR 组）
                if (!string.IsNullOrEmpty(allocation.RealtimeUserId))
                {
                    await _delivery.NotifyAgentNewConversationAsync(allocation.RealtimeUserId, conversation.Id, chatUser.Nickname);
                }

                _logger.LogInformation("Realtime 会话创建成功: ChatUserId={ChatUserId}, ConversationId={ConversationId}, Broadcast={Broadcast}, Members={Count}",
                    chatUser.ChatUserId, conversationId, csConfig.EnableBroadcast, memberIds.Count + 1);
            }

            return conversationId;
        }

        /// <summary>发送群聊 session_init 系统消息（Realtime 会话初始化通知）。</summary>
        public async Task SendGroupSessionInitAsync(string agentUserId, string conversationId, string reason)
        {
            if (!long.TryParse(conversationId, out var convId)) return;
            await _messages.SendSystemAsync(convId,
                SystemMessageType.SessionInit,
                System.Text.Json.JsonSerializer.Serialize(new { type = "session_init", groupId = conversationId, reason, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }),
                agentUserId);
        }

        /// <summary>发送群聊文本消息（Realtime 普通消息）。</summary>
        public async Task SendGroupTextAsync(string agentUserId, string conversationId, string text)
        {
            if (!long.TryParse(conversationId, out var convId)) return;
            await _messages.SendAsync(new RealtimeMessage
            {
                ConversationId = convId,
                SenderId = agentUserId,
                ContentType = "text",
                Content = text,
            });
        }

        /// <summary>
        /// 确保分配的客服有 Realtime 用户 ID（Realtime userId），缺失则生成并持久化。
        /// </summary>
        public void EnsureAgentUserId(AgentAllocationResult allocation)
        {
            if (!allocation.Success || !allocation.AgentId.HasValue)
                return;

            // 已有 userId，直接返回
            if (!string.IsNullOrEmpty(allocation.RealtimeUserId))
                return;

            // 查询 Member 信息
            var member = _memberService.GetById(allocation.AgentId.Value);

            if (member == null) return;

            // 生成并持久化 userId
            var userId = RealtimeIdGenerator.GenerateAgentId(member.Id);
            _memberService.UpdateRealtimeUserId(member.Id, userId);

            allocation.RealtimeUserId = userId;

            _logger.LogInformation("客服 Realtime 用户 ID 生成: MemberId={MemberId}, UserId={UserId}",
                member.Id, userId);
        }
    }
}
