using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Models.Dto;
using SbAgentHub.Services;
using System;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 聊天认证（Widget 公开接口）
/// </summary>
[ApiController, Route("/api/[controller]/[action]")]
public class ChatAuthController : Controller
{
    private readonly ChatAuthService _service;
    private readonly OpenImMessageService _messageService;
    private readonly QueueService _queueService;
    private readonly VisitorFormService _visitorFormService;
    private readonly ChatPlatformService _platformService;
    private readonly MemberService _memberService;

    public ChatAuthController(ChatAuthService service, OpenImMessageService messageService,
        QueueService queueService, VisitorFormService visitorFormService,
        ChatPlatformService platformService, MemberService memberService)
    {
        _service = service;
        _messageService = messageService;
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
        request.IpAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
        request.UserAgent ??= Request.Headers["User-Agent"].ToString();
        return await _service.VisitorAuthAsync(request);
    }

    /// <summary>
    /// SDK 端获取 IM Token（FormData，匹配 openim-core-js SDK）
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> GetImToken([FromForm] GetImTokenFormRequest request)
        => await _service.GetImTokenForSdkAsync(request, ExtractDomain(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"].ToString());

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
