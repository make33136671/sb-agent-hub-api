using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 会话管理（Admin 后台）
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class ConversationController : Controller
{
    private readonly ConversationService _service;

    public ConversationController(ConversationService service)
    {
        _service = service;
    }

    /// <summary>
    /// 会话列表
    /// </summary>
    [HttpGet]
    public async Task<ResultData> List(string keyword = "", int? agentId = null,
        int page = 1, int pageSize = 20)
        => await _service.GetListAsync(keyword, agentId, page, pageSize);
}
