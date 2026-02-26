using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 客服工作台
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class KefuController : Controller
{
    private readonly KefuService _service;

    public KefuController(KefuService service)
    {
        _service = service;
    }

    /// <summary>
    /// 我的访客列表
    /// </summary>
    [HttpGet]
    public ResultData MyVisitors(string keyword = "", int page = 1, int pageSize = 50)
        => _service.GetMyVisitors(keyword, page, pageSize);

    /// <summary>
    /// 更新客服状态
    /// </summary>
    [HttpPost]
    public ResultData UpdateStatus([FromForm] OnlineStatus status)
        => _service.UpdateAgentStatus(status);

    /// <summary>
    /// 转接访客
    /// </summary>
    [HttpPost]
    public ResultData TransferVisitor([FromForm] int chatUserId, [FromForm] int targetAgentId)
        => _service.TransferVisitor(chatUserId, targetAgentId);

    /// <summary>
    /// 可转接的客服列表
    /// </summary>
    [HttpGet]
    public ResultData TransferableAgents()
        => _service.GetTransferableAgents();

    /// <summary>
    /// 当前客服状态
    /// </summary>
    [HttpGet]
    public ResultData MyStatus()
        => _service.GetMyStatus();
}
