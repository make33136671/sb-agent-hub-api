using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Sb.Apps.AgentHub.Services;
using Sb.Apps.AgentHub.Models;

namespace Sb.Apps.AgentHub.WebApi.Controllers;

/// <summary>
/// 认证与账户管理（Admin + Member 共用）
/// </summary>
[ApiController,
    Authorize(AuthenticationSchemes = "JwtAdmin"),
    Route("[controller]/[action]")]
public class AuthController : Controller
{
    private readonly AuthService _authService;
    private readonly FileUploadService _fileUploadService;
    private readonly ChatAuthService _chatAuthService;

    public AuthController(AuthService authService, FileUploadService fileUploadService, ChatAuthService chatAuthService)
    {
        _authService = authService;
        _fileUploadService = fileUploadService;
        _chatAuthService = chatAuthService;
    }

    /// <summary>
    /// 统一登录（Admin / Member）
    /// </summary>
    [HttpPost(Name = "Login"), AllowAnonymous, EnableRateLimiting("login")]
    public async Task<ResultData> Login([FromForm] string username, [FromForm] string password)
        => await _authService.UnifiedLoginAsync(username, password);

    /// <summary>
    /// 获取当前登录用户信息
    /// </summary>
    [HttpGet(Name = "GetInfo")]
    public async Task<ResultData> GetInfo()
        => await _authService.GetInfoAsync();

    /// <summary>
    /// 获取当前用户权限菜单
    /// </summary>
    [HttpGet]
    public ResultData MenuAll()
        => _authService.GetMenus();

    /// <summary>
    /// 退出登录
    /// </summary>
    [HttpPost, AllowAnonymous]
    public IActionResult Logout() => Content("2");

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost]
    public ResultData ChangePassword([FromForm] string oldPassword, [FromForm] string newPassword)
        => _authService.ChangePassword(oldPassword, newPassword);

    /// <summary>
    /// 修改头像
    /// </summary>
    [HttpPost]
    public ResultData ChangeAvatar([FromForm] string avatar)
        => _authService.ChangeAvatar(avatar);

    /// <summary>
    /// 修改个人资料
    /// </summary>
    [HttpPost]
    public ResultData UpdateProfile([FromForm] string name)
        => _authService.UpdateProfile(name);

    /// <summary>
    /// 获取客服 Token（Member 调用）
    /// </summary>
    [HttpGet]
    public async Task<ResultData> GetAgentToken(int memberId, bool forceNew = false)
        => await _chatAuthService.GetAgentTokenAsync(memberId, forceNew);

    /// <summary>
    /// 上传文件
    /// </summary>
    [HttpPost, AllowAnonymous]
    public ResultData Upload(IFormFile file)
        => _fileUploadService.Upload(file);
}
