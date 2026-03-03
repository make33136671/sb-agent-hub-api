using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Models.Dto;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 聊天认证（Widget 公开接口）
/// </summary>
[ApiController, Route("/api/[controller]/[action]")]
public class ChatAuthController : Controller
{
    private readonly ChatAuthService _service;
    private readonly OpenImMessageService _messageService;

    public ChatAuthController(ChatAuthService service, OpenImMessageService messageService)
    {
        _service = service;
        _messageService = messageService;
    }

    /// <summary>
    /// 访客认证 — Widget 初始化
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> VisitorAuth([FromBody] VisitorAuthRequest request)
        => await _service.VisitorAuthAsync(request);

    /// <summary>
    /// SDK 端获取 IM Token（FormData，匹配 openim-core-js SDK）
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> GetImToken([FromForm] GetImTokenFormRequest request)
        => await _service.GetImTokenForSdkAsync(request);

    /// <summary>
    /// 刷新 OpenIM Token
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> RefreshImToken([FromBody] RefreshImTokenRequest request)
        => await _service.RefreshImTokenAsync(request.ChatUserId);

    /// <summary>
    /// 刷新会话 — 容灾恢复（重新获取 Token + 重新分配客服）
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> RefreshSession([FromBody] RefreshSessionRequest request)
        => await _service.RefreshSessionAsync(request);

    /// <summary>
    /// 消息已读回执 — Chat 客户端收到客服消息后调用，发送 message_read 系统消息通知客服
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> MessageRead([FromBody] MessageReadRequest request)
    {
        if (string.IsNullOrEmpty(request.ChatUserId) || string.IsNullOrEmpty(request.AgentImUserId))
            return ResultData.No("参数不完整");

        var payload = new
        {
            type = SystemMessageTypes.MessageRead,
            visitorId = request.ChatUserId,
            conversationId = request.ConversationId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        await _messageService.SendSystemMessageAsync(
            request.ChatUserId,
            request.AgentImUserId,
            SystemMessageTypes.MessageRead,
            payload,
            "[消息已读]"
        );

        return ResultData.Ok();
    }

    /// <summary>
    /// 访客离线通知 — 由客户端 sendBeacon 调用，服务端代发 visitor_offline 系统消息
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> VisitorOffline([FromBody] VisitorOfflineRequest request)
    {
        if (string.IsNullOrEmpty(request.ChatUserId) || string.IsNullOrEmpty(request.AgentImUserId))
            return ResultData.No("参数不完整");

        var payload = new
        {
            type = SystemMessageTypes.VisitorOffline,
            visitorId = request.ChatUserId,
            reason = request.Reason ?? "window_close",
            sessionDuration = request.SessionDuration,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        await _messageService.SendSystemMessageAsync(
            request.ChatUserId,
            request.AgentImUserId,
            SystemMessageTypes.VisitorOffline,
            payload,
            "[访客离线]"
        );

        return ResultData.Ok();
    }
}
