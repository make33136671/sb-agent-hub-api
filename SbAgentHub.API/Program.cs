using SbAgentHub.Core;
using SbAgentHub.Core.Middleware;
using SbAgentHub.Services.BackgroundJobs;
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

            // 启动前归档旧日志
            ArchiveOldLogs();

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

            // 定时任务
            builder.Services.AddHostedService<MessageCleanupJob>();
            builder.Services.AddHostedService<QueueCleanupJob>();
            builder.Services.AddHostedService<QuotaExpirationJob>();

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

        /// <summary>
        /// 将 logs 目录下的旧日志文件归档到以日期时间命名的子文件夹
        /// </summary>
        private static void ArchiveOldLogs()
        {
            var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
            if (!Directory.Exists(logsDir)) return;

            var logFiles = Directory.GetFiles(logsDir, "*.*", SearchOption.TopDirectoryOnly);
            if (logFiles.Length == 0) return;

            var archiveDir = Path.Combine(logsDir, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(archiveDir);

            foreach (var file in logFiles)
            {
                var fileName = Path.GetFileName(file);
                File.Move(file, Path.Combine(archiveDir, fileName));
            }
        }
    }
}
