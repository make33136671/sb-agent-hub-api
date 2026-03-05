using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 套餐模板管理
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class QuotaPackageController : Controller
{
    private readonly QuotaPackageService _service;

    public QuotaPackageController(QuotaPackageService service)
    {
        _service = service;
    }

    /// <summary>
    /// 套餐列表
    /// </summary>
    [HttpGet]
    public ResultData List(bool enabledOnly = false)
    {
        return _service.GetPackageList(enabledOnly);
    }

    /// <summary>
    /// 套餐详情
    /// </summary>
    [HttpGet]
    public ResultData Detail(int id)
    {
        return _service.GetPackageDetail(id);
    }

    /// <summary>
    /// 创建套餐
    /// </summary>
    [HttpPost]
    public ResultData Create([FromBody] CreatePackageRequest req)
    {
        return _service.CreatePackage(req.PackageName, req.PackageKey, req.Description,
            req.Price, req.PricePeriod, req.QuotaSettings, req.SortOrder, req.IsEnabled);
    }

    /// <summary>
    /// 更新套餐
    /// </summary>
    [HttpPost]
    public ResultData Update([FromBody] UpdatePackageRequest req)
    {
        return _service.UpdatePackage(req.Id, req.PackageName, req.Description,
            req.Price, req.PricePeriod, req.QuotaSettings, req.SortOrder, req.IsEnabled);
    }

    /// <summary>
    /// 删除套餐
    /// </summary>
    [HttpPost]
    public ResultData Delete(int id)
    {
        return _service.DeletePackage(id);
    }

    /// <summary>
    /// 应用套餐到站长
    /// </summary>
    [HttpPost]
    public ResultData Apply([FromBody] ApplyPackageRequest req)
    {
        return _service.ApplyPackage(req.PackageId, req.MemberId);
    }

    /// <summary>
    /// 套餐对比
    /// </summary>
    [HttpGet]
    public ResultData Comparison()
    {
        return _service.GetPackageComparison();
    }
}

public class CreatePackageRequest
{
    public string PackageName { get; set; } = "";
    public string PackageKey { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string PricePeriod { get; set; } = "month";
    public string QuotaSettings { get; set; } = "{}";
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class UpdatePackageRequest
{
    public int Id { get; set; }
    public string PackageName { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string PricePeriod { get; set; } = "month";
    public string QuotaSettings { get; set; } = "{}";
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class ApplyPackageRequest
{
    public int PackageId { get; set; }
    public int MemberId { get; set; }
}
