using System;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder of Service Collection for Application Insights for Kubernetes.
    /// </summary>
    internal class KubernetesServiceCollectionBuilder : IKubernetesServiceCollectionBuilder
    {
        private readonly IClusterEnvironmentCheck _clusterCheck;
        private readonly Action<AppInsightsForKubernetesOptions>? _customizeOptions;
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        /// <summary>
        /// Construction for <see cref="KubernetesServiceCollectionBuilder"/>.
        /// </summary>
        /// <param name="customizeOptions">An optional delegate to overwrite app insights for kubernetes options.</param>
        /// <param name="clusterCheck">A service to check if the current process is inside a K8s cluster. This is intended to be used by tests. 
        /// A default checker will be provided when null.
        /// </param>
        public KubernetesServiceCollectionBuilder(
            Action<AppInsightsForKubernetesOptions>? customizeOptions,
            IClusterEnvironmentCheck? clusterCheck)
        {
            _customizeOptions = customizeOptions;
            _clusterCheck = clusterCheck ?? new ClusterEnvironmentCheck();
        }

        /// <summary>
        /// Register Kubernetes related service into the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collector to inject the services into.</param>
        /// <returns>Returns the service collector with services injected.</returns>
        public IServiceCollection RegisterServices(IServiceCollection serviceCollection)
        {
            if (_clusterCheck.IsInCluster)
            {
                if (serviceCollection == null)
                {
                    throw new ArgumentNullException(nameof(serviceCollection));
                }
                serviceCollection.AddTransient<IClusterEnvironmentCheck, ClusterEnvironmentCheck>();
                ConfigureOptions(serviceCollection);

                RegisterCommonServices(serviceCollection);
                RegisterSettingsProvider(serviceCollection);
                RegisterK8sEnvironmentFactory(serviceCollection);
                serviceCollection.AddSingleton<ITelemetryInitializer, KubernetesTelemetryInitializer>();

                _logger.LogDebug("Application Insights Kubernetes injected the service successfully.");
                return serviceCollection;
            }
            else
            {
                // Avoid dependency injection container resolving failure when running out of a K8s cluster.
                serviceCollection.AddScoped<IK8sInfoService, K8sInfoServiceStub>();

                _logger.LogError("Application is not running inside a Kubernetes cluster.");
                return serviceCollection;
            }
        }

        private void ConfigureOptions(IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions();
            serviceCollection.AddOptions<AppInsightsForKubernetesOptions>().Configure<IConfiguration>((opt, configuration) =>
            {
                configuration.GetSection(AppInsightsForKubernetesOptions.SectionName).Bind(opt);
                _customizeOptions?.Invoke(opt);
            });
        }

        private static void RegisterCommonServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITelemetryKeyCache, TelemetryKeyCache>();
            serviceCollection.AddSingleton<SDKVersionUtils>(_ => SDKVersionUtils.Instance);
            serviceCollection.AddSingleton<IK8sClientService>(_ => K8sClientService.Instance);
            serviceCollection.AddSingleton<IContainerIdHolder, ContainerIdHolder>();

            serviceCollection.AddScoped<IPodInfoManager, PodInfoManager>();
            serviceCollection.AddScoped<IContainerStatusManager, ContainerStatusManager>();
            serviceCollection.AddScoped<IK8sInfoService, K8sInfoService>();
        }

        /// <summary>
        /// Registers settings provider for querying K8s proxy.
        /// </summary>
        /// <param name="serviceCollection"></param>
        protected virtual void RegisterSettingsProvider(IServiceCollection serviceCollection)
        {
            // This happens before any of the other IContainerIdProvider. See comments below for details.
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<IContainerIdProvider, EnvironmentVariableContainerIdProvider>());    // Environment variable - works for both Linux & Windows;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Matchers are dependencies of the container id providers.
                serviceCollection.AddScoped<CGroupV1Matcher>();
                serviceCollection.AddScoped<DockerEngineMountInfoMatcher>();
                serviceCollection.AddScoped<ContainerDMountInfoMatcher>();

                // Notes: pay attention to the order. Injecting uses the order of registering in this case.
                // For example, EnvironmentVariableContainerIdProvider will take precedence, then CGroupContainerIdProvider on Linux.
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<IContainerIdProvider, CGroupContainerIdProvider>());                 // Then CGroupV1
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<IContainerIdProvider, ContainerDMountInfoContainerIdProvider>());    // Then ContainerD
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<IContainerIdProvider, DockerEngineMountInfoContainerIdProvider>());  // Then DockerEngine
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Notes: pay attention to the order. Injecting uses the order of registering in this case.
                // For example, EnvironmentVariableContainerIdProvider will take precedence, then EmptyContainerIdProvider on Windows.
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<IContainerIdProvider, EmptyContainerIdProvider>());
            }
            else
            {
                _logger.LogError("Unsupported OS.");
            }

            serviceCollection.TryAddScoped<IContainerIdNormalizer, ContainerIdNormalizer>();

            // Notes: pay attention to the order. Injecting uses the order of registering in this case.
            // For backward compatibility, $APPINSIGHTS_KUBERNETES_POD_NAME has been agreed upon to allow customize pod name with downward API.
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<IPodNameProvider, UserSetPodNameProvider>());
            // $Hostname will be overwritten by Kubernetes to reveal pod name: https://kubernetes.io/docs/concepts/containers/container-environment/#container-information.
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Scoped<IPodNameProvider, HostNamePodNameProvider>());
        }

        /// <summary>
        /// Registers K8s environment factory.
        /// </summary>
        protected virtual void RegisterK8sEnvironmentFactory(IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<IK8sEnvironmentFactory, K8sEnvironmentFactory>();
            serviceCollection.TryAddSingleton<IK8sEnvironmentHolder>(_ => K8sEnvironmentHolder.Instance);
            serviceCollection.AddHostedService<K8sInfoBackgroundService>();
        }
    }
}
