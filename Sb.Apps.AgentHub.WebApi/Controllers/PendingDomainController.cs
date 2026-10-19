using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 待审核域名管理
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class PendingDomainController : Controller
{
    private readonly PendingDomainService _service;

    public PendingDomainController(PendingDomainService service)
    {
        _service = service;
    }

    /// <summary>
    /// 待审核域名列表
    /// </summary>
    [HttpGet]
    public ResultData List(int? platformId = null, string field = "", string order = "",
        int page = 1, int pageSize = 20)
        => _service.GetPendingList(platformId, field, order, page, pageSize);

    /// <summary>
    /// 获取指定平台的待审核域名
    /// </summary>
    [HttpGet]
    public ResultData ListByPlatform(int platformId)
        => _service.GetListByPlatform(platformId);

    /// <summary>
    /// 批准域名（加入白名单）
    /// </summary>
    [HttpPost]
    public ResultData Approve([FromForm] int id)
        => _service.Approve(id);

    /// <summary>
    /// 批量批准域名
    /// </summary>
    [HttpPost]
    public ResultData ApproveBatch([FromForm] int[] ids)
        => _service.ApproveBatch(ids);

    /// <summary>
    /// 拒绝域名
    /// </summary>
    [HttpPost]
    public ResultData Reject([FromForm] int id)
        => _service.Reject(id);

    /// <summary>
    /// 批量拒绝域名
    /// </summary>
    [HttpPost]
    public ResultData RejectBatch([FromForm] int[] ids)
        => _service.RejectBatch(ids);
}
