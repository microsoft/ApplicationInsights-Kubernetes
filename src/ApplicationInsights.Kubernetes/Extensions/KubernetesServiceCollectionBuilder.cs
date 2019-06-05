using System;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder of Service Collection for Application Insights for Kubernetes.
    /// </summary>
    public class KubernetesServiceCollectionBuilder : IKubernetesServiceCollectionBuilder
    {
        private readonly Func<bool> _isRunningInKubernetes;
        private readonly Inspect _diagnosticSource = Inspect.Instance;

        /// <summary>
        /// Construction for <see cref="KubernetesServiceCollectionBuilder"/>.
        /// </summary>
        /// <param name="isRunningInKubernetes"></param>
        public KubernetesServiceCollectionBuilder(Func<bool> isRunningInKubernetes)
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

                serviceCollection.AddSingleton<ITelemetryInitializer, KubernetesTelemetryInitializer>();
                _diagnosticSource.LogDebug("Application Insights Kubernetes injected the service successfully.");
                return serviceCollection;
            }
            else
            {
                _diagnosticSource.LogWarning("Application is not running inside a Kubernetes cluster.");
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
                _diagnosticSource.LogError("Unsupported OS.");
            }
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sEnvironmentFactory>();
        }
    }
}
