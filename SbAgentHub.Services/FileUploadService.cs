using Microsoft.AspNetCore.Http;
using SbAgentHub.Core;
using SbAgentHub.Core.Attribute;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;

namespace SbAgentHub.Services
{
    [AutoDependencyInjection]
    public class FileUploadService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileUploadService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ResultData Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return ResultData.No("请选择要上传的文件");

            var config = AppGlobal.AppSettings.UploadConfig;

            // 校验文件大小
            var maxBytes = config.MaxFileSizeMB * 1024 * 1024;
            if (file.Length > maxBytes)
                return ResultData.No($"文件大小不能超过 {config.MaxFileSizeMB}MB");

            // 校验文件扩展名
            var extension = Path.GetExtension(file.FileName);
            if (config.AllowedExtensions != null && config.AllowedExtensions.Count > 0)
            {
                if (!config.AllowedExtensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                    return ResultData.No($"不允许上传 {extension} 类型的文件");
            }

            // 拼接保存路径
            var relativePath = config.UploadPath;
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{relativePath}");
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            // GUID 重命名，保存文件
            var fileName = Guid.NewGuid().ToString("N") + extension;
            var filePath = Path.Combine(savePath, fileName);

            using (var stream = File.Create(filePath))
            {
                file.CopyTo(stream);
            }

            return ResultData.Ok("上传成功", new
            {
                fileName = $"{relativePath}{fileName}",
                absfileName = $"{_httpContextAccessor.HttpContext.RequestHostWithScheme()}{relativePath}{fileName}"
            });
        }
    }
}
