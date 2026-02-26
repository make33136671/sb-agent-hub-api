using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using SbAgentHub.Services;
using SbAgentHub.Models;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 认证与账户管理
/// </summary>
[ApiController,
    Authorize(AuthenticationSchemes = "JwtAdmin"),
    Route("[controller]/[action]")]
public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    public IHttpContextAccessor _httpContextAccessor;
    public AdminService _adminService;
    public FileUploadService _fileUploadService;
    public AuthController(ILogger<AuthController> logger, IHttpContextAccessor httpContextAccessor, AdminService adminService, FileUploadService fileUploadService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _adminService = adminService;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// 登录管理系统
    /// </summary>
    [HttpPost(Name = "Login"), AllowAnonymous, EnableRateLimiting("login")]
    public async Task<ResultData> Login([FromForm] string username, [FromForm] string password) => await _adminService.LoginAsync(username, password);

    /// <summary>
    /// 获取当前登录的管理员信息
    /// </summary>
    [HttpGet(Name = "GetInfo")]
    public async Task<ResultData> GetInfo() => await _adminService.GetInfoAsync();

    /// <summary>
    /// 退出管理系统
    /// </summary>
    [HttpPost, AllowAnonymous]
    public IActionResult Logout()
    {
        return Content("2");
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost]
    public ResultData ChangePassword([FromForm] string oldPassword, [FromForm] string newPassword) => _adminService.ChangePassword(oldPassword, newPassword);

    /// <summary>
    /// 修改头像
    /// </summary>
    [HttpPost]
    public async Task<ResultData> ChangeAvatar([FromForm] string avatar) => await _adminService.ChangeAvatarAsync(avatar);

    /// <summary>
    /// 修改个人资料
    /// </summary>
    [HttpPost]
    public async Task<ResultData> UpdateProfile([FromForm] string name) => await _adminService.UpdateProfileAsync(name);

    /// <summary>
    /// 上传文件
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public ResultData Upload(IFormFile file) => _fileUploadService.Upload(file);
}
