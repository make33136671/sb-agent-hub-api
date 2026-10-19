using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;
using System;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 非法接入日志
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class AccessViolationController : Controller
{
    private readonly AccessViolationService _service;

    public AccessViolationController(AccessViolationService service)
    {
        _service = service;
    }

    /// <summary>
    /// 非法接入日志列表
    /// </summary>
    [HttpGet]
    public ResultData List(int? ownerId = null, int? platformId = null,
        string reason = "", DateTime? startDate = null, DateTime? endDate = null,
        string field = "", string order = "", int page = 1, int pageSize = 20)
        => _service.GetList(ownerId, platformId, reason, startDate, endDate, field, order, page, pageSize);

    /// <summary>
    /// 统计概览
    /// </summary>
    [HttpGet]
    public ResultData Stats(int? ownerId = null, int days = 7)
        => _service.GetStats(ownerId, days);
}
