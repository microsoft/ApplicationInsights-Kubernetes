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
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        /// <summary>
        /// Construction for <see cref="KubernetesServiceCollectionBuilder"/>.
        /// </summary>
        /// <param name="isRunningInKubernetes">A function that returns true when running inside Kubernetes.</param>
        public KubernetesServiceCollectionBuilder(
            Func<bool> isRunningInKubernetes)
        {
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
                if (serviceCollection == null)
                {
                    throw new ArgumentNullException(nameof(serviceCollection));
                }
                InjectCommonServices(serviceCollection);
                InjectChangableServices(serviceCollection);

#if NETSTANDARD2_0
                serviceCollection.AddSingleton<ITelemetryInitializer>(p =>
                {
                    var userConfig = p.GetRequiredService<IOptions<AppInsightsForKubernetesOptions>>();
                    var envFactory = p.GetRequiredService<IK8sEnvironmentFactory>();
                    var sdkVersionUtils = p.GetRequiredService<SDKVersionUtils>();
                    return userConfig.Value.DisableCounters ?
                        new KubernetesTelemetryInitializer(envFactory, userConfig, sdkVersionUtils) :
                        new KubernetesTelemetryInitializerExt(envFactory, userConfig, sdkVersionUtils);
                });
#else
                serviceCollection.AddSingleton<ITelemetryInitializer, KubernetesTelemetryInitializer>();
#endif
                _logger.LogDebug("Application Insights Kubernetes injected the service successfully.");
                return serviceCollection;
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
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sEnvironmentFactory>();
        }
    }
}
