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

    public ChatAuthController(ChatAuthService service)
    {
        _service = service;
    }

    /// <summary>
    /// 访客认证 — Widget 初始化
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> VisitorAuth([FromBody] VisitorAuthRequest request)
        => await _service.VisitorAuthAsync(request);

    /// <summary>
    /// 刷新 OpenIM Token
    /// </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ResultData> RefreshImToken([FromBody] RefreshImTokenRequest request)
        => await _service.RefreshImTokenAsync(request.ChatUserId);
}
