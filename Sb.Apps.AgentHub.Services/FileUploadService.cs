using FreeSql;
using Microsoft.AspNetCore.Http;
using Sb.Apps.AgentHub.Core;
using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;

namespace Sb.Apps.AgentHub.Services
{
    [AutoDependencyInjection]
    public class FileUploadService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFreeSql _fsql;
        private readonly QuotaService _quotaService;

        public FileUploadService(
            IHttpContextAccessor httpContextAccessor,
            IFreeSql fsql,
            QuotaService quotaService)
        {
            _httpContextAccessor = httpContextAccessor;
            _fsql = fsql;
            _quotaService = quotaService;
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
                absfileName = $"{_httpContextAccessor.HttpContext!.RequestHostWithScheme()}{relativePath}{fileName}"
            });
        }

        /// <summary>
        /// 带存储配额校验的文件上传
        /// </summary>
        public ResultData UploadWithQuota(IFormFile file, int ownerId, int platformId = 0,
            string uploaderId = "", string uploaderType = "visitor")
        {
            if (file == null || file.Length == 0)
                return ResultData.No("请选择要上传的文件");

            // 存储配额校验
            var storageCheck = _quotaService.CheckQuota(ownerId, "storage_quota_mb");
            long fileSizeMB = file.Length / (1024 * 1024);
            if (storageCheck.Remaining <= fileSizeMB)
                return ResultData.No($"存储空间不足，剩余 {storageCheck.Remaining}MB。");

            // 执行上传
            var result = Upload(file);
            if (result.Code != 0) return result;

            // 记录文件信息
            var fileType = GetFileType(Path.GetExtension(file.FileName));
            var storedPath = ((dynamic)result.Data).fileName;

            _fsql.Insert(new FileRecord
            {
                OwnerId = ownerId,
                PlatformId = platformId,
                UploaderId = uploaderId,
                UploaderType = uploaderType,
                FileName = file.FileName,
                StoredPath = storedPath,
                FileSize = file.Length,
                FileType = fileType,
                MimeType = file.ContentType ?? "",
                CreateTime = DateTime.Now,
                IsDeleted = false
            }).ExecuteAffrows();

            return result;
        }

        /// <summary>
        /// 获取站长的存储使用量（MB）
        /// </summary>
        public long GetStorageUsageMB(int ownerId)
        {
            var totalBytes = (long)_fsql.Select<FileRecord>()
                .Where(f => f.OwnerId == ownerId)
                .Sum(f => f.FileSize);
            return totalBytes / (1024 * 1024);
        }

        private static string GetFileType(string extension)
        {
            extension = extension?.ToLower() ?? "";
            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" or ".bmp" => "image",
                ".mp3" or ".wav" or ".ogg" or ".aac" => "voice",
                ".mp4" or ".avi" or ".mov" or ".webm" => "video",
                _ => "file"
            };
        }
    }
}
