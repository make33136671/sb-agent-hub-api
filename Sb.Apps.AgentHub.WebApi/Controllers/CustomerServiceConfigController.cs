using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 客服配置管理
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class CustomerServiceConfigController : Controller
{
    private readonly CustomerServiceConfigService _service;

    public CustomerServiceConfigController(CustomerServiceConfigService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取当前 Owner 的客服配置
    /// </summary>
    [HttpGet]
    public ResultData GetConfig() => _service.GetConfig();

    /// <summary>
    /// 保存客服配置
    /// </summary>
    [HttpPost]
    public ResultData SaveConfig([FromForm] CustomerServiceConfig config) => _service.SaveConfig(config);
}
