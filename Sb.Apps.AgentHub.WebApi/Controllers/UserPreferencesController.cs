using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sb.Apps.AgentHub.Models;
using Sb.Apps.AgentHub.Services;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 用户偏好设置
/// </summary>
[ApiController, Authorize(AuthenticationSchemes = "JwtAdmin"), Route("[controller]/[action]")]
public class UserPreferencesController : Controller
{
    private readonly UserPreferencesService _service;

    public UserPreferencesController(UserPreferencesService service)
    {
        _service = service;
    }

    [HttpGet]
    public ResultData Get() => _service.GetPreferences();

    [HttpPost]
    public ResultData Save([FromForm] UserPreferences prefs) => _service.SavePreferences(prefs);

    /// <summary>
    /// 获取声音设置
    /// </summary>
    [HttpGet]
    public ResultData GetSoundSettings() => _service.GetSoundSettings();

    /// <summary>
    /// 保存声音设置
    /// </summary>
    [HttpPost]
    public ResultData SaveSoundSettings([FromForm] SoundSettingsDto dto) => _service.SaveSoundSettings(dto);

    /// <summary>
    /// 获取可用提示音列表
    /// </summary>
    [HttpGet]
    public ResultData GetAvailableSounds() => _service.GetAvailableSounds();
}
