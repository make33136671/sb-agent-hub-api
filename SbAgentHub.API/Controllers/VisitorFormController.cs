using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 访客信息收集表单管理（站长端）
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class VisitorFormController : Controller
{
    private readonly VisitorFormService _service;

    public VisitorFormController(VisitorFormService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取表单列表
    /// </summary>
    [HttpGet]
    public ResultData List(int? platformId = null)
        => _service.GetFormList(platformId);

    /// <summary>
    /// 获取表单详情
    /// </summary>
    [HttpGet]
    public ResultData Detail(int id)
        => _service.GetFormDetail(id);

    /// <summary>
    /// 创建表单
    /// </summary>
    [HttpPost]
    public ResultData Create([FromBody] CreateVisitorFormRequest request)
        => _service.CreateForm(request.PlatformId, request.FormName, request.FormFields, request.IsRequired);

    /// <summary>
    /// 更新表单
    /// </summary>
    [HttpPost]
    public ResultData Update([FromBody] UpdateVisitorFormRequest request)
        => _service.UpdateForm(request.Id, request.FormName, request.FormFields, request.IsRequired, request.IsEnabled);

    /// <summary>
    /// 删除表单
    /// </summary>
    [HttpPost]
    public ResultData Delete([FromForm] int id)
        => _service.DeleteForm(id);

    /// <summary>
    /// 获取表单提交记录
    /// </summary>
    [HttpGet]
    public ResultData Submissions(int formId, int page = 1, int pageSize = 20)
        => _service.GetSubmissions(formId, page, pageSize);
}

public class CreateVisitorFormRequest
{
    public int PlatformId { get; set; }
    public string FormName { get; set; } = "访客信息";
    public string FormFields { get; set; } = "[]";
    public bool IsRequired { get; set; } = true;
}

public class UpdateVisitorFormRequest
{
    public int Id { get; set; }
    public string FormName { get; set; } = "";
    public string FormFields { get; set; } = "[]";
    public bool IsRequired { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
}
