using System;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder of Service Collection for Application Insights for Kubernetes.
    /// </summary>
    public class KubernetesServiceCollectionBuilder : IKubernetesServiceCollectionBuilder
    {
        private readonly Func<bool> _isRunningInKubernetes;
        private readonly AppInsightsForKubernetesOptions _options;
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        /// <summary>
        /// Construction for <see cref="KubernetesServiceCollectionBuilder"/>.
        /// </summary>
        /// <param name="isRunningInKubernetes">A function that returns true when running inside Kubernetes.</param>
        /// <param name="options"></param>
        public KubernetesServiceCollectionBuilder(
            Func<bool> isRunningInKubernetes,
            IOptions<AppInsightsForKubernetesOptions> options)
        {
            _isRunningInKubernetes = isRunningInKubernetes ?? throw new ArgumentNullException(nameof(isRunningInKubernetes));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Register Kubernetes related service into the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collector to inject the services into.</param>
        /// <returns>Returns the service collector with services injected.</returns>
        public IServiceCollection RegisterServices(IServiceCollection serviceCollection)
        {
            if (_isRunningInKubernetes())
            {
                if (serviceCollection == null)
                {
                    throw new ArgumentNullException(nameof(serviceCollection));
                }
                RegisterCommonServices(serviceCollection);
                RegisterSettingsProvider(serviceCollection);
                RegisterK8sEnvironmentFactory(serviceCollection);
                serviceCollection.AddSingleton<ITelemetryInitializer, KubernetesTelemetryInitializer>();
                RegisterPerformanceCounterTelemetryInitializer(serviceCollection);

                _logger.LogDebug("Application Insights Kubernetes injected the service successfully.");
                return serviceCollection;
            }
            else
            {
                _logger.LogWarning("Application is not running inside a Kubernetes cluster.");
                return serviceCollection;
            }
        }

        private void RegisterPerformanceCounterTelemetryInitializer(IServiceCollection serviceCollection)
        {
            if (!_options.DisablePerformanceCounters)
            {
                serviceCollection.AddSingleton<ITelemetryInitializer, SimplePerformanceCounterTelemetryInitializer>();
            }
            else
            {
                _logger.LogDebug("{initializerName} is disabled by configuration.", nameof(SimplePerformanceCounterTelemetryInitializer));
            }
        }

        private static void RegisterCommonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITelemetryKeyCache, TelemetryKeyCache>();
            serviceCollection.AddSingleton<KubeHttpClientFactory>();
            serviceCollection.AddSingleton<K8sQueryClientFactory>();
            serviceCollection.AddSingleton<SDKVersionUtils>(p => SDKVersionUtils.Instance);
        }

        /// <summary>
        /// Registers setttings provider for querying K8s proxy.
        /// </summary>
        /// <param name="serviceCollection"></param>
        protected virtual void RegisterSettingsProvider(IServiceCollection serviceCollection)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpClientSettingsProvider>();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpSettingsWinContainerProvider>();
            }
            else
            {
                _logger.LogError("Unsupported OS.");
            }
        }

        /// <summary>
        /// Registers K8s environment factory.
        /// </summary>
        protected virtual void RegisterK8sEnvironmentFactory(IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sEnvironmentFactory>();
    }
}
