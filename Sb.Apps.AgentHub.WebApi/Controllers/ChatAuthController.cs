using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Models.Dto;
using Sb.Apps.AgentHub.Services;
using Sb.Framework.Chat.Realtime.Conversations;
using Sb.Framework.Chat.Realtime.Messages;
using Sb.Framework.Chat.Realtime.ReadState;
using System;
using System.Text.Json;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 聊天认证（Widget 公开接口）
/// </summary>
[ApiController, Route("/api/[controller]/[action]")]
public class ChatAuthController : Controller
{
    private readonly ChatAuthService _service;
    private readonly SdkResponseBuilder _sdkResponseBuilder;
    private readonly IFreeSql _fsql;
    private readonly IConversationService _conversations;
    private readonly IMessageService _messages;
    private readonly IReadStateService _readState;
    private readonly QueueService _queueService;
    private readonly VisitorFormService _visitorFormService;
    private readonly ChatPlatformService _platformService;
    private readonly MemberService _memberService;

    public ChatAuthController(ChatAuthService service, SdkResponseBuilder sdkResponseBuilder, IFreeSql fsql,
        IConversationService conversations, IMessageService messages, IReadStateService readState,
        QueueService queueService, VisitorFormService visitorFormService,
        ChatPlatformService platformService, MemberService memberService)
    {
        _service = service;
        _sdkResponseBuilder = sdkResponseBuilder;
        _fsql = fsql;
        _conversations = conversations;
        _messages = messages;
        _readState = readState;
        _queueService = queueService;
        _visitorFormService = visitorFormService;
        _platformService = platformService;
        _memberService = memberService;
    }

    /// <summary>
    /// 访客认证 — Widget 初始化
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> VisitorAuth([FromBody] VisitorAuthRequest request)
    {
        // 从请求头提取来源域名
        request.RequestDomain = ExtractDomain();
        request.IpAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        request.UserAgent ??= Request.Headers["User-Agent"].ToString();
        return await _service.VisitorAuthAsync(request);
    }

    /// <summary>
    /// SDK 端获取 Token（FormData，匹配 realtime-core-js SDK）
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> GetToken([FromForm] GetTokenFormRequest request)
        => await _sdkResponseBuilder.GetTokenForSdkAsync(request, ExtractDomain(),
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            Request.Headers["User-Agent"].ToString());

    /// <summary>
    /// 刷新 Token
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> RefreshToken([FromBody] RefreshTokenRequest request)
        => await _service.RefreshTokenAsync(request.ChatUserId);

    /// <summary>
    /// 刷新会话 — 容灾恢复（重新获取 Token + 重新分配客服）
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> RefreshSession([FromBody] RefreshSessionRequest request)
        => await _service.RefreshSessionAsync(request);

    /// <summary>
    /// 消息已读回执 — Chat 客户端收到客服消息后调用，更新 Realtime 已读游标
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> MessageRead([FromBody] MessageReadRequest request)
    {
        if (string.IsNullOrEmpty(request.ChatUserId))
            return ResultData.No("参数不完整");

        var conversationId = ResolveConversationId(request.ConversationId, request.ChatUserId, request.AgentUserId);
        if (conversationId <= 0)
            return ResultData.No("会话不存在");

        await _readState.MarkReadAsync(conversationId, request.ChatUserId, long.MaxValue);
        return ResultData.Ok();
    }

    /// <summary>
    /// 更新用户信息 — 客服/访客同步头像和昵称到 Realtime
    /// </summary>
    [HttpPost, AllowAnonymous]
    public ResultData UpdateUserInfo([FromForm] string platformKey, [FromForm] string deviceId, [FromForm] string? userId, [FromForm] string? nickname, [FromForm] string? avatar)
    {
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var uid))
        {
            var sets = new List<string>();
            var args = new Dictionary<string, object?> { ["uid"] = uid };
            if (!string.IsNullOrEmpty(nickname)) { sets.Add("\"Nickname\" = @nickname"); args["nickname"] = nickname; }
            if (!string.IsNullOrEmpty(avatar)) { sets.Add("\"Avatar\" = @avatar"); args["avatar"] = avatar; }
            if (sets.Count > 0)
            {
                _fsql.Ado.ExecuteNonQuery(
                    $"UPDATE sb_agenthub_chat_user SET {string.Join(", ", sets)} WHERE \"Id\" = @uid", args);
            }
        }
        return ResultData.Ok();
    }

    /// <summary>
    /// 访客离线通知 — 由客户端 sendBeacon 调用，向会话发 visitor_offline 系统消息
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> VisitorOffline([FromBody] VisitorOfflineRequest request)
    {
        if (string.IsNullOrEmpty(request.ChatUserId))
            return ResultData.No("参数不完整");

        var conversationId = ResolveConversationId("", request.ChatUserId, request.AgentUserId);
        if (conversationId <= 0)
            return ResultData.Ok("已忽略");

        var payload = new
        {
            type = SystemMessageTypes.VisitorOffline,
            visitorId = request.ChatUserId,
            reason = request.Reason ?? "window_close",
            sessionDuration = request.SessionDuration,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        await _messages.SendSystemAsync(
            conversationId,
            SystemMessageType.Prefix + SystemMessageTypes.VisitorOffline,
            JsonSerializer.Serialize(payload),
            request.ChatUserId);

        return ResultData.Ok();
    }

    /// <summary>解析会话 ID：优先用传入的 conversationId；否则定位访客-客服 direct 会话。</summary>
    private long ResolveConversationId(string conversationId, string visitorId, string agentUserId)
    {
        if (long.TryParse(conversationId, out var id) && id > 0)
            return id;

        if (string.IsNullOrEmpty(visitorId) || string.IsNullOrEmpty(agentUserId))
            return 0;

        // 查访客-客服共同所在的 direct 会话
        var memberRows = _fsql.Select<Sb.Framework.Chat.Realtime.Storage.RtConversationMember>()
            .Where(m => m.UserId == visitorId || m.UserId == agentUserId)
            .ToList(m => new { m.ConversationId, m.UserId });

        var visitorConvs = memberRows.Where(m => m.UserId == visitorId).Select(m => m.ConversationId).ToHashSet();
        return memberRows
            .Where(m => m.UserId == agentUserId && visitorConvs.Contains(m.ConversationId))
            .Select(m => m.ConversationId)
            .FirstOrDefault();
    }

    /// <summary>
    /// 查询排队位置
    /// </summary>
    [HttpGet, AllowAnonymous]
    public ResultData QueuePosition(string chatUserId)
    {
        if (string.IsNullOrEmpty(chatUserId))
            return ResultData.No("chatUserId 不能为空");
        var info = _queueService.GetPosition(chatUserId);
        if (info == null)
            return ResultData.Ok(new { isQueued = false });
        return ResultData.Ok(new { isQueued = true, info.Position, info.TotalWaiting, info.EstimatedWaitMinutes });
    }

    /// <summary>
    /// 取消排队
    /// </summary>
    [HttpPost, AllowAnonymous]
    public ResultData CancelQueue([FromBody] CancelQueueRequest request)
    {
        if (string.IsNullOrEmpty(request?.ChatUserId))
            return ResultData.No("chatUserId 不能为空");
        _queueService.Cancel(request.ChatUserId);
        return ResultData.Ok("已取消排队");
    }

    /// <summary>
    /// 获取平台的表单配置（访客端）
    /// </summary>
    [HttpGet, AllowAnonymous]
    public ResultData GetVisitorForm(string platformKey)
    {
        if (string.IsNullOrEmpty(platformKey))
            return ResultData.No("platformKey 不能为空");

        var platform = _service.GetPlatformByKey(platformKey);
        if (platform == null)
            return ResultData.No("平台不存在!");

        var form = _visitorFormService.GetActiveForm(platform.OwnerId, platform.Id);
        if (form == null)
            return ResultData.Ok(new { hasForm = false });

        return ResultData.Ok(new
        {
            hasForm = true,
            formId = form.Id,
            formName = form.FormName,
            formFields = form.FormFields,
            isRequired = form.IsRequired
        });
    }

    /// <summary>
    /// 提交访客表单（访客端）
    /// </summary>
    [HttpPost, AllowAnonymous]
    public ResultData SubmitVisitorForm([FromBody] SubmitVisitorFormRequest request)
    {
        if (request == null || request.FormId <= 0)
            return ResultData.No("参数无效!");
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        return _visitorFormService.SubmitForm(request.FormId, request.ChatUserId, request.FormData, ip);
    }

    /// <summary>
    /// 通过网站Key和客服工号查询客服基本信息
    /// </summary>
    [HttpGet, AllowAnonymous]
    public ResultData GetAgentInfo(string platformKey, string agentNo)
    {
        if (string.IsNullOrEmpty(platformKey) || string.IsNullOrEmpty(agentNo))
            return ResultData.No("参数不完整");

        var platform = _platformService.GetByPlatformKey(platformKey);
        if (platform == null)
            return ResultData.No("平台不存在或未启用");

        return _memberService.GetAgentPublicInfo(platform.OwnerId, agentNo);
    }

    /// <summary>
    /// 从 Origin 或 Referer 请求头提取域名
    /// </summary>
    private string ExtractDomain()
    {
        // 优先 Origin
        var origin = Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            try { return new Uri(origin).Host; } catch { }
        }

        // 其次 Referer
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            try { return new Uri(referer).Host; } catch { }
        }

        return "";
    }
}
