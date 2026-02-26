using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using SbAgentHub.Core.Authentication.Jwt;
using SbAgentHub.Core.Expand;
using SbAgentHub.Models;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace SbAgentHub.API.Extensions
{
    public static class JwtSetup
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            Assembly jwtToken = Assembly.Load("SbAgentHub.Core");
            var jwtTokenAssembly = jwtToken.ExportedTypes.Where(t => t.Name == "JwtToken").FirstOrDefault();
            services.AddSingleton(jwtTokenAssembly);

            services
                .AddAuthentication("JwtAdmin")
                .AddJwtBearer("JwtAdmin", options =>
                {
                    IConfigurationSection jwtConfig = configuration.GetSection("JwtAdmin");
                    jwtConfig.Bind(options);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]))
                    };
                });

            services
                .AddAuthentication("JwtApi")
                .AddJwtBearer("JwtApi", options =>
                {
                    IConfigurationSection jwtConfig = configuration.GetSection("JwtApi");
                    jwtConfig.Bind(options);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]))
                    };

                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents()
                    {
                        OnTokenValidated = context =>
                        {
                            var cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
                            JwtToken? jwtToken = context.HttpContext.RequestServices.GetService<JwtToken>();

                            string appToken = context.Request.Headers["Authorization"].ToString();
                            appToken = appToken.Replace("Bearer ", "");

                            System.Security.Claims.ClaimsPrincipal claims = jwtToken.DecryptToken("JwtApi", appToken);
                            string userIdStr = claims.FindFirstValue("Id") ?? "";
                            string token = cache.Get<string>(userIdStr);

                            if (token != null && token != appToken)
                            {
                                context.Fail("请重新登录App");
                            }
                            else if (token == null)
                            {
                                cache.Set<string>(userIdStr, appToken);
                            }

                            IFreeSql? freeSql = context.HttpContext.RequestServices.GetService<IFreeSql>();
                            if (freeSql != null && !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                            {
                                Member member = freeSql.Select<Member>().Where(c => c.Id == userId).First();
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
