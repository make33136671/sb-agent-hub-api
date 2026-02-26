using SbAgentHub.Core;
using SbAgentHub.Core.Middleware;
using Serilog;
using SbAgentHub.API.Extensions;
using System.Text.Json.Serialization;

namespace SbAgentHub.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigurationManager configuration = builder.Configuration;

            AppGlobal.AppSettings = configuration.Get<AppSettings>();

            // Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            builder.Host.UseSerilog();
            builder.Logging.ClearProviders();

            // 注册所有服务
            builder.Services.AddFreeSql(configuration);
            builder.Services.AddJwtAuthentication(configuration);
            builder.Services.AddBusinessServices();
            builder.Services.AddCacheService();
            builder.Services.AddLoginRateLimiter();
            builder.Services.AddMySqlHealthCheck(configuration);

            builder.Services.AddCorsPolicy(configuration);
            builder.Services.AddSwaggerDoc();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllersWithViews(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                options.Conventions.Add(new AdminRoutePrefixConvention());
            }).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            // 中间件管道
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseCors();
            app.UseRateLimiter();
            app.UseSerilogRequestLogging();
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<QueryCountMiddleware>();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSwaggerDoc();

            // 端点映射
            app.MapHealthChecks("/health");
            app.MapControllers();

            app.Run();
        }
    }
}
