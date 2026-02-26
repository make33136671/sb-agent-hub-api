using Microsoft.AspNetCore.Http;

namespace SbAgentHub.Core.Expand
{
    public static class ExpandHttpContext
    {
        /// <summary>
        /// 当前请求的Host域名
        /// </summary>
        public static string RequestHost(this HttpContext context)
        {
            string host = $"{context.Request.Host}";
            return host;
        }

        /// <summary>
        /// 当前请求的Host域名包含Scheme
        /// </summary>
        public static string RequestHostWithScheme(this HttpContext context)
        {
            string host = $"{context.Request.Scheme}://{context.Request.Host}";
            return host;
        }

        /// <summary>
        /// 当前请求的Origin
        /// </summary>
        public static string RequestOrigin(this HttpContext context)
        {
            context.Request.Headers.TryGetValue("origin", out Microsoft.Extensions.Primitives.StringValues origin);

            if (origin.Count<=0) {
                context.Request.Headers.TryGetValue("referer", out origin);
            }

            if (origin.Count <= 0)
            {
                return "";
            }

            string host = string.Join(",", origin);

            host = host.Replace("https://", "").Replace("http://", "").Replace("/","");

            return host;
        }

      
    }
}