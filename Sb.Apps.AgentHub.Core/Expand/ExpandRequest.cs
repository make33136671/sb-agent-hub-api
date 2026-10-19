using Microsoft.AspNetCore.Http;

namespace Sb.Apps.AgentHub.Core.Expand
{
    /// <summary>
    /// HttpContext 请求相关扩展方法
    /// </summary>
    public static class ExpandHttpContext
    {
        /// <summary>
        /// 当前请求的Host域名
        /// </summary>
        /// <param name="context">Http 上下文</param>
        /// <returns>Host 域名</returns>
        /// <exception cref="ArgumentNullException">context 为 null 时抛出</exception>
        public static string RequestHost(this HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            string host = $"{context.Request.Host}";
            return host;
        }

        /// <summary>
        /// 当前请求的Host域名包含Scheme
        /// 优先级：ForceHttps 配置 > X-Forwarded-Proto 头 > Request.Scheme
        /// </summary>
        /// <param name="context">Http 上下文</param>
        /// <returns>带 Scheme 的 Host 域名</returns>
        /// <exception cref="ArgumentNullException">context 为 null 时抛出</exception>
        public static string RequestHostWithScheme(this HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var request = context.Request;

            // 1. 最高优先级：ForceHttps 配置强制使用 https
            if (AppGlobal.AppSettings?.ForceHttps == true)
            {
                return $"https://{request.Host}";
            }

            // 2. 读取 X-Forwarded-Proto（Nginx 等反向代理会设置此头）
            string scheme = request.Scheme;
            if (request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto)
                && !string.IsNullOrEmpty(forwardedProto))
            {
                scheme = forwardedProto.ToString().Split(',')[0].Trim().ToLowerInvariant();
            }

            string host = $"{scheme}://{request.Host}";
            return host;
        }

        /// <summary>
        /// 当前请求的Origin
        /// </summary>
        /// <param name="context">Http 上下文</param>
        /// <returns>Origin 域名（不含协议和路径）</returns>
        /// <exception cref="ArgumentNullException">context 为 null 时抛出</exception>
        public static string RequestOrigin(this HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            context.Request.Headers.TryGetValue("origin", out Microsoft.Extensions.Primitives.StringValues origin);

            if (origin.Count <= 0)
            {
                context.Request.Headers.TryGetValue("referer", out origin);
            }

            if (origin.Count <= 0)
            {
                return "";
            }

            string host = string.Join(",", origin.ToArray());

            host = host.Replace("https://", "").Replace("http://", "").Replace("/", "");

            return host;
        }
    }
}
