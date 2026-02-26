using FreeSql;
using SbAgentHub.Core.Middleware;
using SbAgentHub.Models;
using Serilog;
using System.Diagnostics;

namespace SbAgentHub.API.Extensions
{
    public static class FreeSqlSetup
    {
        public static IServiceCollection AddFreeSql(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection dbConfig = configuration.GetSection("DbConfig");
            string server = dbConfig.GetValue("server", "127.0.0.1");
            string port = dbConfig.GetValue("port", "3306");
            string database = dbConfig.GetValue("database", "sb");
            string user = dbConfig.GetValue("user", "sb");
            string password = dbConfig.GetValue("password", "sb");
            bool autoSyncStructure = dbConfig.GetValue<bool>("AutoSyncStructure", false);
            bool monitorSqlCommandForLog = dbConfig.GetValue<bool>("MonitorSqlCommandForLog", false);
            int slowQueryThresholdMs = dbConfig.GetValue<int>("SlowQueryThresholdMs", 200);

            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                               .UseConnectionString(FreeSql.DataType.MySql, $"server={server}; port={port}; user id={user}; password={password}; database={database}; sslMode=PREFERRED; ")
                               .UseAutoSyncStructure(autoSyncStructure) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
                               .Build();

            // 性能监控: 慢查询日志 + 查询计数
            fsql.Aop.CommandBefore += (s, e) =>
            {
                e.States["Stopwatch"] = Stopwatch.StartNew();
            };

            fsql.Aop.CommandAfter += (s, e) =>
            {
                var sw = (Stopwatch)e.States["Stopwatch"];
                sw.Stop();
                var ms = sw.ElapsedMilliseconds;

                // 查询计数 (QueryCountMiddleware 统计每个请求的查询次数)
                QueryCountMiddleware.IncrementQueryCount();

                if (ms > slowQueryThresholdMs)
                {
                    Log.Warning("慢查询 ({Ms}ms, 阈值: {Threshold}ms): {Sql}", ms, slowQueryThresholdMs, e.Command.CommandText);
                }
                else if (monitorSqlCommandForLog)
                {
                    Log.Debug("查询 ({Ms}ms): {Sql}", ms, e.Command.CommandText);
                }
            };

            // 全局软删除过滤
            fsql.GlobalFilter.Apply<Admin>("IsDeleted", a => a.IsDeleted == false);
            fsql.GlobalFilter.Apply<Menu>("IsDeleted", a => a.IsDeleted == false);
            fsql.GlobalFilter.Apply<AdminRole>("IsDeleted", a => a.IsDeleted == false);
            fsql.GlobalFilter.Apply<Member>("IsDeleted", a => a.IsDeleted == false);
            fsql.GlobalFilter.Apply<ChatUser>("IsDeleted", a => a.IsDeleted == false);
            fsql.GlobalFilter.Apply<ChatPlatform>("IsDeleted", a => a.IsDeleted == false);
            Log.Information("已应用全局软删除过滤");

            fsql.UseJsonMap();

            services.AddSingleton<IFreeSql>(fsql);

            return services;
        }
    }
}
