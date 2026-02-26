using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.API.Controllers;

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
}
