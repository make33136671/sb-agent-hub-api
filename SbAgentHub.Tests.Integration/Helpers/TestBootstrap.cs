using FreeSql;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SbAgentHub.Core;
using SbAgentHub.Models;
using SbAgentHub.Services;

namespace SbAgentHub.Tests.Integration.Helpers
{
    /// <summary>
    /// 测试基础设施 — 初始化 AppGlobal、ICacheService、IFreeSql
    /// </summary>
    public static class TestBootstrap
    {
        private static readonly object _lock = new();
        private static bool _initialized;
        private static ICacheService? _cacheService;
        private static IFreeSql? _freeSql;

        /// <summary>
        /// 初始化 AppGlobal.AppSettings（线程安全，只执行一次）
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;

                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.test.json", optional: false)
                    .Build();

                var appSettings = new AppSettings();
                config.Bind(appSettings);
                AppGlobal.AppSettings = appSettings;

                _initialized = true;
            }
        }

        /// <summary>
        /// 获取内存缓存服务（单例）
        /// </summary>
        public static ICacheService GetCacheService()
        {
            EnsureInitialized();
            if (_cacheService == null)
            {
                var memoryCache = new MemoryCache(new MemoryCacheOptions());
                _cacheService = new CacheMemoryCacheService(memoryCache);
            }
            return _cacheService;
        }

        /// <summary>
        /// 获取 IFreeSql 实例（单例，连接 MySQL）
        /// </summary>
        public static IFreeSql GetFreeSql()
        {
            EnsureInitialized();
            if (_freeSql != null) return _freeSql;

            var db = AppGlobal.AppSettings.DbConfig;
            var connStr = $"Data Source={db.Server};Port={db.Port};User ID={db.User};Password={db.Password};Initial Catalog={db.Database};Charset=utf8mb4;SslMode=none;AllowPublicKeyRetrieval=true";

            _freeSql = new FreeSqlBuilder()
                .UseConnectionString(DataType.MySql, connStr)
                .UseAutoSyncStructure(false)
                .Build();

            // 应用与生产相同的全局软删除过滤
            _freeSql.GlobalFilter.Apply<Admin>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<Menu>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<AdminRole>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<Member>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<ChatUser>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<ChatPlatform>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<CustomerServiceConfig>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<ChatUserLoginRecord>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<ClientErrorLog>("IsDeleted", a => a.IsDeleted == false);
            _freeSql.GlobalFilter.Apply<UserPreferences>("IsDeleted", a => a.IsDeleted == false);

            return _freeSql;
        }
    }
}
