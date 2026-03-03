using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Services;
using SbAgentHub.Models;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 公开文件上传（供 Widget / IM SDK 使用）
/// </summary>
[ApiController, AllowAnonymous, Route("/api/[controller]/[action]")]
public class FileController : Controller
{
    private readonly FileUploadService _fileUploadService;

    public FileController(FileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    [HttpPost]
    public ResultData Upload(IFormFile file)
        => _fileUploadService.Upload(file);
}
