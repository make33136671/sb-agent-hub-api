using Microsoft.AspNetCore.Http;

namespace Sb.Apps.AgentHub.Core.Expand
{
    public static class ExpandImageUrl
    {
        /// <summary>
        /// 相对路径 → 绝对 URL（如 /upload/xxx.png → http://host/upload/xxx.png）
        /// 已是绝对 URL 则原样返回
        /// 优先级：ForceHttps 配置 > X-Forwarded-Proto 头 > Request.Scheme
        /// </summary>
        public static string ToAbsoluteUrl(this string? path, HttpContext ctx)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (path.StartsWith("http://") || path.StartsWith("https://")) return path;
            var request = ctx.Request;

            // 1. 最高优先级：ForceHttps 配置强制使用 https
            if (AppGlobal.AppSettings?.ForceHttps == true)
            {
                return $"https://{request.Host}{(path.StartsWith("/") ? "" : "/")}{path}";
            }

            // 2. 读取 X-Forwarded-Proto（Nginx 等反向代理会设置此头）
            string scheme = request.Scheme;
            if (request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto)
                && !string.IsNullOrEmpty(forwardedProto))
            {
                scheme = forwardedProto.ToString().Split(',')[0].Trim().ToLowerInvariant();
            }

            return $"{scheme}://{request.Host}{(path.StartsWith("/") ? "" : "/")}{path}";
        }

        /// <summary>
        /// 绝对 URL → 相对路径（如 http://host/upload/xxx.png → /upload/xxx.png）
        /// 已是相对路径则原样返回
        /// </summary>
        public static string ToRelativePath(this string? url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            if (!url.StartsWith("http://") && !url.StartsWith("https://")) return url;
            try
            {
                var uri = new Uri(url);
                return uri.AbsolutePath;
            }
            catch
            {
                return url;
            }
        }
    }
}
