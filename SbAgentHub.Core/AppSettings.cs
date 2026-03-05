namespace SbAgentHub.Core
{
    public static class AppGlobal
    {
        public static AppSettings AppSettings { get; set; }
    }

    public class AppSettings
    {
        /// <summary>
        /// AppName
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 启用Debug模式
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// 开启Swagger
        /// </summary>
        public bool UseSwagger { get; set; } = false;

        /// <summary>
        /// OpenIM 配置
        /// </summary>
        public OpenIMConfig OpenIM { get; set; } = new();


        /// <summary>
        /// 缓存配置
        /// </summary>
        public CacheConfig CacheConfig { get; set; } = new();

        /// <summary>
        /// 密码存储配置
        /// </summary>
        public PasswordConfig PasswordConfig { get; set; } = new();

        /// <summary>
        /// 程序启动端口信息
        /// </summary>
        public WebHost WebHost { get; set; }

        /// <summary>
        /// Admin Jwt加密授权信息
        /// </summary>
        public JwtAdmin JwtAdmin { get; set; }

        /// <summary>
        /// Api Jwt加密授权信息
        /// </summary>
        public JwtAPI JwtAPI { get; set; }

        /// <summary>
        /// 数据库配置信息
        /// </summary>
        public DbConfig DbConfig { get; set; }

        /// <summary>
        /// 登录频率限制配置
        /// </summary>
        public RateLimitConfig RateLimitConfig { get; set; } = new();


        /// <summary>
        /// 文件上传配置
        /// </summary>
        public UploadConfig UploadConfig { get; set; } = new();

        /// <summary>
        /// 定时作业开启
        /// </summary>
        public bool TaskRun { get; set; }
    }

    public class WebHost
    {
        /// <summary>
        /// 运行地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 运行端口号
        /// </summary>
        public int Port { get; set; }
    }

    public class JwtAdmin
    {
        /// <summary>
        ///
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string Expires { get; set; }
    }

    public class JwtAPI
    {
        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Expires { get; set; }
    }

    public class DbConfig
    {
        /// <summary>
        /// 数据库服务器地址
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 数据库用户名
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 数据库密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 自动同步数据库结构,正式环境中慎用
        /// </summary>
        public string AutoSyncStructure { get; set; }

        /// <summary>
        /// 是否在控制台输出Sql日志
        /// </summary>
        public string MonitorSqlCommandForLog { get; set; }

        /// <summary>
        /// 慢查询阈值（毫秒），超过此值的查询将记录为Warning日志
        /// </summary>
        public int SlowQueryThresholdMs { get; set; } = 200;
    }


    /// <summary>
    /// 文件上传配置
    /// </summary>
    public class UploadConfig
    {
        /// <summary>
        /// 最大文件大小（MB）
        /// </summary>
        public long MaxFileSizeMB { get; set; } = 10;

        /// <summary>
        /// 允许的文件扩展名列表，为空时不限制
        /// </summary>
        public List<string> AllowedExtensions { get; set; } = new();

        /// <summary>
        /// 上传文件保存的相对路径
        /// </summary>
        public string UploadPath { get; set; } = "/upload/";
    }

    /// <summary>
    /// 登录频率限制配置
    /// </summary>
    public class RateLimitConfig
    {
        /// <summary>
        /// 时间窗口大小（秒）
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// 窗口内最大请求数
        /// </summary>
        public int PermitLimit { get; set; } = 10;
    }

    /// <summary>
    /// 缓存系统配置
    /// </summary>
    public class CacheConfig
    {
        /// <summary>
        /// 是否启用缓存（false 时将使用 No-Op 缓存实现）
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// 缓存提供程序: "Memory" | "Redis"（仅当 Enabled 为 true 时有效）
        /// </summary>
        public string Provider { get; set; } = "Memory";

        /// <summary>
        /// Redis 配置（仅 Provider 为 "Redis" 时生效）
        /// </summary>
        public RedisConfig Redis { get; set; } = new();
    }

    /// <summary>
    /// 密码存储配置
    /// 注意：此配置在系统安装后不应修改，否则会导致密码验证失败
    /// </summary>
    public class PasswordConfig
    {
        /// <summary>
        /// 密码存储模式: "Encrypted"（加密） | "PlainText"（明文）
        /// 默认为加密模式，强烈推荐生产环境使用加密模式
        /// 警告：不兼容修改 - 一旦选定不能修改
        /// </summary>
        public string StorageMode { get; set; } = "Encrypted";
    }

    /// <summary>
    /// Redis 连接配置
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// Redis 连接字符串
        /// </summary>
        public string Connection { get; set; } = "localhost:6379";

        /// <summary>
        /// 键名前缀，用于多应用共享 Redis 时隔离
        /// </summary>
        public string InstanceName { get; set; } = "SB:";
    }

    /// <summary>
    /// OpenIM 配置
    /// </summary>
    public class OpenIMConfig
    {
        /// <summary>
        /// OpenIM Server HTTP API 地址（服务端连接用）
        /// </summary>
        public string ImApiUrl { get; set; } = "";

        /// <summary>
        /// OpenIM Server WebSocket 地址（服务端连接用）
        /// </summary>
        public string ImWsUrl { get; set; } = "";

        /// <summary>
        /// 客户端 IM HTTP API 地址（下发给前端/Widget，未配置时回退到 ImApiUrl）
        /// </summary>
        public string ClientApiUrl { get; set; } = "";

        /// <summary>
        /// 客户端 IM WebSocket 地址（下发给前端/Widget，未配置时回退到 ImWsUrl）
        /// </summary>
        public string ClientWsUrl { get; set; } = "";

        /// <summary>
        /// 管理员用户ID
        /// </summary>
        public string AdminUserId { get; set; } = "imAdmin";

        /// <summary>
        /// API 密钥
        /// </summary>
        public string Secret { get; set; } = "";
    }
}
