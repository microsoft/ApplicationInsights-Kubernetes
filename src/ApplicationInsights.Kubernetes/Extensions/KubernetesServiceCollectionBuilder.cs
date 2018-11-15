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
            ILogger<IKubernetesServiceCollectionBuilder> logger)
        {
            _logger = logger;
            _isRunningInKubernetes = isRunningInKubernetes ?? throw new ArgumentNullException(nameof(isRunningInKubernetes));
        }

        /// <summary>
        /// Injects Kubernetes related service into the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collector to inject the services into.</param>
        /// <returns>Returns the service collector with services injected.</returns>
        public IServiceCollection InjectServices(IServiceCollection serviceCollection)
        {
            if (_isRunningInKubernetes())
            {
                IServiceCollection services = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
                InjectCommonServices(services);
                InjectChangableServices(services);

                services.AddSingleton<ITelemetryInitializer, KubernetesTelemetryInitializer>();
                _logger?.LogDebug("Application Insights Kubernetes injected the service successfully.");
                return services;
            }
            else
            {
                _logger?.LogWarning("Application is not running inside a Kubernetes cluster.");
                return serviceCollection;
            }
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

                return services;
            }
            else
            {
                _logger?.LogWarning("Application is not running inside a Kubernetes cluster.");
                return serviceCollection;
            }
        }

        private static void InjectCommonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<KubeHttpClientFactory>();
            serviceCollection.AddSingleton<K8sQueryClientFactory>();
            serviceCollection.AddSingleton<SDKVersionUtils>(SDKVersionUtils.Instance);
        }

        /// <summary>
        /// Injects the services of Application Insights for Kubernetes.
        /// </summary>
        /// <param name="serviceCollection">The service collector to inject the services into.</param>
        protected virtual void InjectChangableServices(IServiceCollection serviceCollection)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider>(p =>
                    new KubeHttpClientSettingsProvider(logger: p.GetService<ILogger<KubeHttpClientSettingsProvider>>()));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider>(p =>
                    new KubeHttpSettingsWinContainerProvider(logger: p.GetService<ILogger<KubeHttpSettingsWinContainerProvider>>()));
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
