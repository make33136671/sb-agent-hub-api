using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;
using System.Collections.Generic;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 配额管理
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class QuotaController : Controller
{
    private readonly QuotaService _service;

    public QuotaController(QuotaService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取配额定义列表
    /// </summary>
    [HttpGet]
    public ResultData Configs(bool enabledOnly = true)
        => _service.GetQuotaConfigs(enabledOnly);

    /// <summary>
    /// 获取站长的配额列表（含使用量）
    /// </summary>
    [HttpGet]
    public ResultData MemberQuotas(int memberId)
        => _service.GetMemberQuotas(memberId);

    /// <summary>
    /// 设置单个配额
    /// </summary>
    [HttpPost]
    public ResultData SetQuota([FromForm] int memberId, [FromForm] string quotaKey,
        [FromForm] int quotaValue, [FromForm] string remark = "")
        => _service.SetMemberQuota(memberId, quotaKey, quotaValue, remark);

    /// <summary>
    /// 批量设置配额
    /// </summary>
    [HttpPost]
    public ResultData BatchSet([FromBody] BatchSetQuotaRequest request)
        => _service.BatchSetMemberQuotas(request.MemberId, request.Quotas, request.Remark);

    /// <summary>
    /// 配额使用概览
    /// </summary>
    [HttpGet]
    public ResultData UsageSummary(int memberId)
        => _service.GetUsageSummary(memberId);

    /// <summary>
    /// 站长选项列表
    /// </summary>
    [HttpGet]
    public ResultData OwnerOptions()
        => _service.GetOwnerOptions();
}

/// <summary>
/// 批量设置配额请求
/// </summary>
public class BatchSetQuotaRequest
{
    public int MemberId { get; set; }
    public List<QuotaItem> Quotas { get; set; } = null!;
    public string Remark { get; set; } = "";
}
