using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 客户端错误日志
/// </summary>
[ApiController, Route("/api/[controller]/[action]")]
public class ClientErrorLogController : Controller
{
    private readonly ClientErrorLogService _service;

    public ClientErrorLogController(ClientErrorLogService service)
    {
        _service = service;
    }

    /// <summary>
    /// 上报客户端错误
    /// </summary>
    [HttpPost, AllowAnonymous]
    public ResultData Report([FromBody] ClientErrorLog log) => _service.Report(log);
}
