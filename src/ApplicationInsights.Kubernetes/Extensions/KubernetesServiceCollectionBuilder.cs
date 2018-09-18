using System;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder of Service Collection for Application Insights for Kubernetes.
    /// </summary>
    public class KubernetesServiceCollectionBuilder : IKubernetesServiceCollectionBuilder
    {
        private readonly ILogger _logger;
        private readonly Func<bool> _isRunningInKubernetes;
        
        /// <summary>
        /// Construction for <see cref="KubernetesServiceCollectionBuilder"/>.
        /// </summary>
        /// <param name="isRunningInKubernetes"></param>
        /// <param name="logger"></param>
        public KubernetesServiceCollectionBuilder(
            Func<bool> isRunningInKubernetes,
            ILogger<KubernetesServiceCollectionBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isRunningInKubernetes = isRunningInKubernetes ?? throw new ArgumentNullException(nameof(isRunningInKubernetes));
        }

        /// <summary>
        /// Injects Kubernetes related service into the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collector to inject the services into.</param>
        /// <param name="timeout">Maximum time to wait for spinning up the container.</param>
        /// <returns>Returns the service collector with services injected.</returns>
        public IServiceCollection InjectServices(IServiceCollection serviceCollection, TimeSpan timeout)
        {
            if (_isRunningInKubernetes())
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
                    _logger.LogDebug("Application Insights Kubernetes injected the service successfully.");
                    return initializer;
                });
                return services;
            }
            else
            {
                _logger.LogWarning("Application is not running inside a Kubernetes cluster.");
                return serviceCollection;
            }
        }

        private static void InjectCommonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<KubeHttpClientFactory>();
            serviceCollection.AddSingleton<K8sQueryClientFactory>();
        }

        /// <summary>
        /// Injects the services of Application Insights for Kubernetes.
        /// </summary>
        /// <param name="serviceCollection">The service collector to inject the services into.</param>
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
