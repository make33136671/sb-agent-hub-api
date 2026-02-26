namespace SbAgentHub.API.Extensions
{
    public static class HealthCheckSetup
    {
        /// <summary>
        /// 注册健康检查（MySQL 连接检测）
        /// </summary>
        public static IServiceCollection AddMySqlHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection dbConfig = configuration.GetSection("DbConfig");
            string connStr = $"server={dbConfig["Server"]}; port={dbConfig["Port"]}; user id={dbConfig["User"]}; password={dbConfig["Password"]}; database={dbConfig["Database"]}; sslMode=PREFERRED;";

            services.AddHealthChecks()
                .AddMySql(connStr, name: "mysql");

            return services;
        }
    }
}
