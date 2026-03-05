using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 接入平台管理
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class ChatPlatformController : Controller
{
    private readonly ChatPlatformService _service;

    public ChatPlatformController(ChatPlatformService service)
    {
        _service = service;
    }

    [HttpGet]
    public ResultData List(string keyword = "", int? ownerId = null, PlatformType? platformType = null,
        bool? isEnabled = null, string field = "", string order = "",
        int page = 1, int pageSize = 20)
        => _service.GetList(keyword, ownerId, platformType, isEnabled, field, order, page, pageSize);

    [HttpGet]
    public ResultData Detail(int id) => _service.Detail(id);

    [HttpPost]
    public ResultData Add([FromForm] ChatPlatform platform) => _service.Create(platform);

    [HttpPost]
    public ResultData Edit([FromForm] ChatPlatform platform) => _service.Edit(platform);

    [HttpPost]
    public ResultData Del([FromForm] int id) => _service.Del(id);

    [HttpPost]
    public ResultData ResetPlatformKey([FromForm] int id) => _service.ResetPlatformKey(id);

    [HttpPost]
    public ResultData ToggleEnabled([FromForm] int id) => _service.ToggleEnabled(id);

    [HttpGet]
    public ResultData MyPlatforms() => _service.GetMyPlatforms();

    [HttpGet]
    public ResultData EnabledPlatforms() => _service.GetEnabledPlatforms();

    [HttpGet]
    public ResultData OwnerOptions() => _service.GetOwnerOptions();

    [HttpGet]
    public ResultData AdminOptions() => _service.GetAdminOptions();

    [HttpPost]
    public ResultData ToggleTakeover([FromForm] int id, [FromForm] int? adminId = null)
        => _service.ToggleTakeover(id, adminId);

    /// <summary>
    /// 获取接入代码/链接
    /// </summary>
    [HttpGet]
    public ResultData AccessCode(int id) => _service.GetAccessCode(id);
}
