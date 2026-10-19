using Sb.Apps.AgentHub.Core.Attribute;
using Sb.Framework.Chat.Realtime.Extensions;
using System.Reflection;

namespace Sb.Apps.AgentHub.WebApi.Extensions
{
    public static class ServiceSetup
    {
        /// <summary>
        /// 注册客服系统业务服务（扫描 Sb.Apps.AgentHub.Services 的 [AutoDependencyInjection]）。
        /// 注意：与框架 SB.Framework 的 [AutoDependencyInjection] 是不同特性，框架装配不会注册客服服务，
        /// 必须保留本扫描；方法名避开框架同名扩展 AddBusinessServices 以防二义性。
        /// 同时注册自建 JwtToken 单例（生成 token 用；验证由框架 JwtBearer 负责，读同一配置节）。
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">应用配置（读 "Chat:Realtime" 节给 Realtime 模块）</param>
        public static IServiceCollection AddAgentBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 自建 JwtToken（Sb.Apps.AgentHub.Core.Authentication.Jwt.JwtToken）— 业务层生成 token 用
            Assembly coreAssembly = Assembly.Load("Sb.Apps.AgentHub.Core");
            var jwtTokenType = coreAssembly.ExportedTypes.FirstOrDefault(t => t.Name == "JwtToken");
            if (jwtTokenType != null)
                services.AddSingleton(jwtTokenType);

            // 自建缓存（Sb.Apps.AgentHub.Services.ICacheService，客服服务注入；
            // 框架缓存是 SB.Framework.Services.ICacheService，两者不同接口并存）
            services.AddCacheService();

            // Realtime 框架模块（SignalR Hub + 消息/会话/在线/已读/媒体服务，Chat:Realtime 节）
            services.AddSbRealtime(configuration);

            Assembly assembly = Assembly.Load("Sb.Apps.AgentHub.Services");

            foreach (var type in assembly.ExportedTypes)
            {
                if (type.FullName == null || !type.FullName.Contains("Sb.Apps.AgentHub.Services"))
                    continue;

                var attr = type.GetCustomAttribute<AutoDependencyInjectionAttribute>();
                if (attr == null || !attr.AutoDependencyInjection)
                    continue;

                services.AddScoped(type);
            }

            return services;
        }
    }
}
