using System;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
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
        public IServiceCollection InjectServices(IServiceCollection serviceCollection, TimeSpan timeout)
        {
            IServiceCollection services = serviceCollection ?? new ServiceCollection();
            InjectCommonServices(services);

            InjectChangableServices(services);

            // Inject the telemetry initializer.
            services.AddSingleton<ITelemetryInitializer>(provider =>
            {
                KubernetesTelemetryInitializer initializer = new KubernetesTelemetryInitializer(
                    provider.GetRequiredService<IK8sEnvironmentFactory>(),
                    timeout,
                    SDKVersionUtils.Instance,
                    provider.GetRequiredService<ILogger<KubernetesTelemetryInitializer>>()
                );
                provider.GetRequiredService<ILogger<KubernetesServiceCollectionBuilder>>()
                    .LogDebug("Application Insights Kubernetes injected the service successfully.");
                return initializer;
            });
            return services;
        }

        private static void InjectCommonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<KubeHttpClientFactory>();
            serviceCollection.AddSingleton<K8sQueryClientFactory>();
        }

        protected virtual void InjectChangableServices(IServiceCollection serviceCollection)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider>(p => new KubeHttpClientSettingsProvider(logger: p.GetService<ILogger<KubeHttpClientSettingsProvider>>()));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider>(p => new KubeHttpSettingsWinContainerProvider(logger: p.GetService<ILogger<KubeHttpSettingsWinContainerProvider>>()));
            }
            else
            {
                // TODO: See if there is a way to get rid of intermediate service provider when getting logger.
                serviceCollection.BuildServiceProvider().GetService<ILogger<KubernetesServiceCollectionBuilder>>().LogError("Unsupported OS.");
            }
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sEnvironmentFactory>();
        }
    }
}
