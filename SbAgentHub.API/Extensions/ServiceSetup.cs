using SbAgentHub.Core.Attribute;
using System.Reflection;

namespace SbAgentHub.API.Extensions
{
    public static class ServiceSetup
    {
        /// <summary>
        /// 自动扫描并注册标记了 [AutoDependencyInjection] 的业务服务
        /// </summary>
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            Assembly assembly = Assembly.Load("SbAgentHub.Services");

            foreach (var type in assembly.ExportedTypes)
            {
                if (!type.FullName.Contains("SbAgentHub.Services"))
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
