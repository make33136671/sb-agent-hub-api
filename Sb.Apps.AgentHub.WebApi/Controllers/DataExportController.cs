using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 数据导出
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class DataExportController : Controller
{
    private readonly DataExportService _service;

    public DataExportController(DataExportService service)
    {
        _service = service;
    }

    /// <summary>
    /// 检查导出权限
    /// </summary>
    [HttpGet]
    public ResultData CheckPermission()
    {
        return ResultData.Ok(new { canExport = _service.CanExport() });
    }

    /// <summary>
    /// 导出聊天记录
    /// </summary>
    [HttpGet]
    public IActionResult ChatRecords(DateTime? startDate, DateTime? endDate, int? platformId)
    {
        if (!_service.CanExport())
            return BadRequest(new { code = -1, message = "未开通数据导出功能!" });

        var (data, fileName) = _service.ExportChatRecords(startDate, endDate, platformId);
        return File(data, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// 导出满意度评价
    /// </summary>
    [HttpGet]
    public IActionResult SatisfactionRatings(DateTime? startDate, DateTime? endDate)
    {
        if (!_service.CanExport())
            return BadRequest(new { code = -1, message = "未开通数据导出功能!" });

        var (data, fileName) = _service.ExportSatisfactionRatings(startDate, endDate);
        return File(data, "text/csv; charset=utf-8", fileName);
    }
}
