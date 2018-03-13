using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public class KubernetesServiceCollectionBuilder : IKubernetesServiceCollectionBuilder
    {
        /// <summary>
        /// Inject Kubernetes related service into the service collection.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public IServiceCollection InjectServices(IServiceCollection serviceCollection)
        {
            IServiceCollection services = serviceCollection ?? new ServiceCollection();
            InjectCommonServices(services);

            InjectChangableServices(services);

            return services;
        }

        private static void InjectCommonServices(IServiceCollection serviceCollection)
        {
            // According to the code, adding logging will not overwrite existing logging classes
            // https://github.com/aspnet/Logging/blob/c821494678a30c323174bea8056f43b93a3ca6f4/src/Microsoft.Extensions.Logging/LoggingServiceCollectionExtensions.cs
            // Becuase it uses 'TryAdd()' extenion method on service collection.
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton<KubeHttpClientFactory>();
            serviceCollection.AddSingleton<K8sQueryClientFactory>();
        }

        protected virtual void InjectChangableServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider>(p => new KubeHttpClientSettingsProvider(logger: p.GetService<ILogger<KubeHttpClientSettingsProvider>>()));
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sEnvironmentFactory>();
        }
    }
}
