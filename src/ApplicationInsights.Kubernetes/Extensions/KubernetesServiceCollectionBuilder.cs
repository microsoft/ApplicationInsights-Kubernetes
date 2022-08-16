using System;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                _logger.LogError("Application is not running inside a Kubernetes cluster.");
                return serviceCollection;
            }
        }

        private void RegisterPerformanceCounterTelemetryInitializer(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITelemetryInitializer, SimplePerformanceCounterTelemetryInitializer>();
        }

        private static void RegisterCommonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITelemetryKeyCache, TelemetryKeyCache>();
            serviceCollection.AddSingleton<KubeHttpClientFactory>();
            serviceCollection.AddSingleton<IK8sQueryClientFactory, K8sQueryClientFactory>();
            serviceCollection.AddSingleton<SDKVersionUtils>(p => SDKVersionUtils.Instance);
        }

        /// <summary>
        /// Registers settings provider for querying K8s proxy.
        /// </summary>
        /// <param name="serviceCollection"></param>
        protected virtual void RegisterSettingsProvider(IServiceCollection serviceCollection)
        {
            // This happens before any of the other IContainerIdProvider. See comments below for details.
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IContainerIdProvider, EnvironmentVariableContainerIdProvider>());    // Environment variable - works for both Linux & Windows;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Matchers are dependencies of the container id providers.
                serviceCollection.AddSingleton<CGroupV1Matcher>();
                serviceCollection.AddSingleton<DockerEngineMountInfoMatcher>();
                serviceCollection.AddSingleton<ContainerDMountInfoMatcher>();

                // Notes: pay attention to the order. Injecting uses the order of registering in this case.
                // For example, EnvironmentVariableContainerIdProvider will take precedence, then CGroupContainerIdProvider on Linux.
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IContainerIdProvider, CGroupContainerIdProvider>());                 // Then CGroupV1
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IContainerIdProvider, ContainerDMountInfoContainerIdProvider>());    // Then ContainerD
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IContainerIdProvider, DockerEngineMountInfoContainerIdProvider>());  // Then DockerEngine

                serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpClientSettingsProvider>();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Notes: pay attention to the order. Injecting uses the order of registering in this case.
                // For example, EnvironmentVariableContainerIdProvider will take precedence, then EmptyContainerIdProvider on Windows.
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IContainerIdProvider, EmptyContainerIdProvider>());
                serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpSettingsWinContainerProvider>();
            }
            else
            {
                _logger.LogError("Unsupported OS.");
            }

            serviceCollection.TryAddSingleton<IContainerIdNormalizer, ContainerIdNormalizer>();

            // Notes: pay attention to the order. Injecting uses the order of registering in this case.
            // For backward compatibility, $APPINSIGHTS_KUBERNETES_POD_NAME has been agreed upon to allow customize pod name with downward API.
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IPodNameProvider, UserSetPodNameProvider>());
            // $Hostname will be overwritten by Kubernetes to reveal pod name: https://kubernetes.io/docs/concepts/containers/container-environment/#container-information.
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IPodNameProvider, HostNamePodNameProvider>());

            serviceCollection.AddSingleton<IPodInfoManager, PodInfoManager>();
        }

        /// <summary>
        /// Registers K8s environment factory.
        /// </summary>
        protected virtual void RegisterK8sEnvironmentFactory(IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sEnvironmentFactory>();
    }
}
