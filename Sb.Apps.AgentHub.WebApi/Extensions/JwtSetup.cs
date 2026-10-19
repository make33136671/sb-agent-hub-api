using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Sb.Apps.AgentHub.Core.Authentication.Jwt;
using Sb.Apps.AgentHub.Core.Expand;
using Sb.Apps.AgentHub.Models;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace Sb.Apps.AgentHub.WebApi.Extensions
{
    public static class JwtSetup
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            Assembly jwtTokenAssembly = Assembly.Load("Sb.Apps.AgentHub.Core");
            var jwtTokenType = jwtTokenAssembly.ExportedTypes.Where(t => t.Name == "JwtToken").FirstOrDefault();
            if (jwtTokenType == null)
                throw new InvalidOperationException("Type 'JwtToken' not found in assembly 'Sb.Apps.AgentHub.Core'.");
            services.AddSingleton(jwtTokenType);

            services
                .AddAuthentication("JwtAdmin")
                .AddJwtBearer("JwtAdmin", options =>
                {
                    IConfigurationSection jwtConfig = configuration.GetSection("JwtAdmin");
                    jwtConfig.Bind(options);

                    var key = jwtConfig["Key"] ?? throw new InvalidOperationException("JwtAdmin:Key is not configured.");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        TryAllIssuerSigningKeys = true
                    };
                })
                .AddJwtBearer("JwtApi", options =>
                {
                    IConfigurationSection jwtConfig = configuration.GetSection("JwtApi");
                    jwtConfig.Bind(options);

                    var key = jwtConfig["Key"] ?? throw new InvalidOperationException("JwtApi:Key is not configured.");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        TryAllIssuerSigningKeys = true
                    };

                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents()
                    {
                        OnTokenValidated = context =>
                        {
                            var cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
                            var jwtToken = context.HttpContext.RequestServices.GetService<JwtToken>();
                            if (jwtToken == null)
                            {
                                context.Fail("JwtToken service is not available.");
                                return Task.CompletedTask;
                            }

                            string appToken = context.Request.Headers["Authorization"].ToString();
                            appToken = appToken.Replace("Bearer ", "");

                            var claims = jwtToken.DecryptToken("JwtApi", appToken);
                            string userIdStr = claims.FindFirstValue("Id") ?? "";
                            string? token = cache?.Get<string>(userIdStr);

                            if (token != null && token != appToken)
                            {
                                context.Fail("请重新登录App");
                            }
                            else if (token == null && cache != null)
                            {
                                cache.Set(userIdStr, appToken);
                            }

                            IFreeSql? freeSql = context.HttpContext.RequestServices.GetService<IFreeSql>();
                            if (freeSql != null && !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                            {
                                var member = freeSql.Select<Member>().Where(c => c.Id == userId).First();
                                if (member != null && member.Status != UserStatus.Active)
                                {
                                    context.Fail("请重新登录App");
                                }
                            }

                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context => Task.CompletedTask,
                        OnForbidden = context => Task.CompletedTask,
                        OnChallenge = context => Task.CompletedTask,
                        OnAuthenticationFailed = context => Task.CompletedTask
                    };
                });

            return services;
        }
    }
}
