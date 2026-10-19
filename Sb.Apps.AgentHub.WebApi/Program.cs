using Microsoft.AspNetCore.HttpOverrides;
using Sb.Apps.AgentHub.Core;
using Sb.Apps.AgentHub.Services;
using Sb.Apps.AgentHub.Services.BackgroundJobs;
using Sb.Apps.AgentHub.WebApi.Extensions;
using SB.Framework.WebApi.Extensions;
using Sb.Framework.Chat.Realtime.Extensions;
using FreeSql;
using FreeSql.DataAnnotations;
using Serilog;
using System.Reflection;

namespace Sb.Apps.AgentHub.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 客服系统专属配置（不属于 SB.Framework 通用底座）
            AgentConfig.Options = builder.Configuration.Get<AgentOptions>() ?? new AgentOptions();

            // 客服系统自建配置（CacheConfig/PasswordConfig/UploadConfig 等，供自建组件使用；
            // 框架通用配置由 AddSbFramework 自行绑定。M0.4 迁移完自建组件后移除）
            AppGlobal.AppSettings = builder.Configuration.Get<AppSettings>() ?? new AppSettings();

            // 配置转发头（支持 Nginx 反向代理后的 X-Forwarded-Proto/For/Host）
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                // 信任所有代理（生产环境建议指定 KnownProxies 或 KnownIPNetworks）
                options.KnownProxies.Clear();
                options.KnownIPNetworks.Clear();
            });

            // SB.Framework 底座一键装配：Serilog/FreeSql/JWT/CORS/Swagger/限流/健康检查/后台任务/Feature 体系
            // （installed.lock 已存在 → 走完整装配分支；ExcludeControllers 排除框架内置 Controller，保留客服定制实现）
            builder.AddSbFramework();

            // 客服系统业务服务（自建 [AutoDependencyInjection] 扫描 + Realtime 模块装配）
            builder.Services.AddAgentBusinessServices(builder.Configuration);

            // 客服系统定时任务（过渡期保留 AddHostedService；后续迁移到框架 BaseBackgroundTask 体系）
            builder.Services.AddHostedService<MessageCleanupJob>();
            builder.Services.AddHostedService<QueueCleanupJob>();
            builder.Services.AddHostedService<QuotaExpirationJob>();
            builder.Services.AddHostedService<VisitorPresenceHandler>();
            builder.Services.AddHostedService<PresenceHealthCheckJob>();

            var app = builder.Build();

            // ★ 转发头中间件必须放在最前面，让后续中间件都能读到正确的 Scheme/Host/IP
            app.UseForwardedHeaders();

            // 建表：同步 Realtime 模块表 + 客服系统所有业务实体表
            // （客服系统未实现 IModuleInitializer，框架 SeedDatabase 无法自动发现，故在此显式扫描同步）
            using (var scope = app.Services.CreateScope())
            {
                var fsql = scope.ServiceProvider.GetRequiredService<IFreeSql>();

                // 1. Realtime 模块表
                var realtimeTypes = new[]
                {
                    typeof(Sb.Framework.Chat.Realtime.Storage.RtConversation),
                    typeof(Sb.Framework.Chat.Realtime.Storage.RtConversationMember),
                    typeof(Sb.Framework.Chat.Realtime.Storage.RtMessage),
                    typeof(Sb.Framework.Chat.Realtime.Storage.RtReadState),
                    typeof(Sb.Framework.Chat.Realtime.Storage.RtMedia),
                };

                // 2. 客服系统业务实体表（扫描 Models 程序集中所有 [Table] 类型）
                var modelsAssembly = Assembly.Load("Sb.Apps.AgentHub.Models");
                var agentEntityTypes = modelsAssembly.GetTypes()
                    .Where(t => t.GetCustomAttributes<TableAttribute>().Any())
                    .ToArray();

                // 合并并同步表结构
                var allTypes = realtimeTypes.Concat(agentEntityTypes).Distinct().ToArray();
                fsql.CodeFirst.SyncStructure(allTypes);
                Log.Information("表结构同步完成，共 {Count} 个实体（Realtime: {RtCount} + 客服系统: {AgentCount}）",
                    allTypes.Length, realtimeTypes.Length, agentEntityTypes.Length);

                // 3. 插入配额配置种子数据
                EnsureQuotaSeedData(fsql);

                // 4. 初始化系统种子数据（角色 + 菜单 + 角色菜单关联）
                new SeedDataService(fsql).EnsureAll();
            }

            // SB.Framework 中间件管道（全局异常/查询计数/静态文件/CORS/限流/鉴权/Swagger/SeedDatabase）
            app.UseSbFramework();

            // Realtime 框架模块端点（SignalR Hub，默认 /hubs/realtime，配置节 Chat:Realtime）
            app.UseSbRealtime();

            app.Run();
        }

        private static void EnsureQuotaSeedData(IFreeSql fsql)
        {
            var seedData = new (string Key, string Name, int Type, int Def, string Unit, string Desc, int Sort)[]
            {
                ("max_platforms", "最大平台数", 0, 999999, "个", "站长可创建的平台数量上限", 1),
                ("max_agents", "最大客服数", 0, 999999, "个", "站长可创建的客服账号上限", 2),
                ("max_concurrent_sessions", "最大并发会话", 0, 999999, "个", "同时在线的访客会话数上限", 3),
                ("max_daily_visitors", "每日访客数", 0, 999999, "人次", "每日新增访客咨询上限", 4),
                ("storage_quota_mb", "存储空间", 2, 999999, "MB", "文件存储配额(MB)", 5),
                ("message_retention_days", "消息保留天数", 0, 999999, "天", "聊天消息保留天数", 6),
            };

            foreach (var (key, name, type, def, unit, desc, sort) in seedData)
            {
                fsql.Ado.ExecuteNonQuery($@"INSERT INTO sb_agenthub_quota_config
                    (""QuotaKey"",""QuotaName"",""QuotaType"",""DefaultValue"",""MinValue"",""MaxValue"",""Unit"",""Description"",""SortOrder"",""IsEnabled"",""IsDeleted"")
                    SELECT '{key}','{name}',{type},{def},0,999999,'{unit}','{desc}',{sort},true,false
                    WHERE NOT EXISTS (SELECT 1 FROM sb_agenthub_quota_config WHERE ""QuotaKey"" = '{key}')");
            }

            Log.Information("配额配置种子数据检查完成");
        }
    }
}
