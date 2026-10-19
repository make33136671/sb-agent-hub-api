using Microsoft.AspNetCore.RateLimiting;
using Sb.Apps.AgentHub.Core;
using Sb.Apps.AgentHub.Models;
using System.Threading.RateLimiting;

namespace Sb.Apps.AgentHub.WebApi.Extensions
{
    public static class RateLimiterSetup
    {
        /// <summary>
        /// 注册登录频率限制（固定窗口策略）
        /// </summary>
        public static IServiceCollection AddLoginRateLimiter(this IServiceCollection services)
        {
            var config = AppGlobal.AppSettings.RateLimitConfig;

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    var result = ResultData.No("请求过于频繁，请稍后再试");
                    await context.HttpContext.Response.WriteAsJsonAsync(result, cancellationToken);
                };
                options.AddFixedWindowLimiter("login", opt =>
                {
                    opt.Window = TimeSpan.FromSeconds(config.WindowSeconds);
                    opt.PermitLimit = config.PermitLimit;
                    opt.QueueLimit = 0;
                    opt.AutoReplenishment = true;
                });
            });

            return services;
        }
    }
}
