using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension method to register Kubernetes Telemetry Initializer.
    /// </summary>
    public static partial class ApplicationInsightsExtensions
    {
        private static readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemetryConfiguration in the dependency injection container with custom options.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="diagnosticLogLevel">Sets the diagnostics log levels for the enricher.</param>
        /// <param name="disableBackgroundService">If true, the Application Insights enricher library will not use any background (hosted) services,
        /// and Kubernetes information will not be fetched automatically. Hosted services are not allowed in some environments, e.g. Azure Function. 
        /// For more information see https://github.com/Azure/azure-functions-host/issues/5447#issuecomment-575368316</param>
        /// <param name="applyOptions">Action to customize the configuration of Application Insights for Kubernetes.</param>
        /// <param name="clusterCheck">Provides a custom implementation to check whether it is inside kubernetes cluster or not.</param>
        /// <returns>The service collection for chaining the next operation.</returns>
        public static IServiceCollection AddApplicationInsightsKubernetesEnricher(
            this IServiceCollection services,
            LogLevel? diagnosticLogLevel = LogLevel.None,
            bool disableBackgroundService = false,
            Action<AppInsightsForKubernetesOptions>? applyOptions = default,
            IClusterEnvironmentCheck? clusterCheck = default)
        {
            diagnosticLogLevel ??= LogLevel.None;   // Default to None.
            if (diagnosticLogLevel != LogLevel.None)
            {
                ApplicationInsightsKubernetesDiagnosticObserver observer = new ApplicationInsightsKubernetesDiagnosticObserver((DiagnosticLogLevel)diagnosticLogLevel);
                ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);
            }

            if (!KubernetesTelemetryInitializerExists(services))
            {
                services.ConfigureKubernetesTelemetryInitializer(applyOptions, clusterCheck, disableBackgroundService);
            }
            return services;
        }

        /// <summary>
        /// Bootstraps Application Insights Kubernetes enricher. This is intended to be used in console application that does not use hosted services.
        /// </summary>
        public static void StartApplicationInsightsKubernetesEnricher(this IServiceProvider serviceProvider)
        {
            IK8sInfoBootstrap? k8sInfoBootstrap = serviceProvider.GetService<IK8sInfoBootstrap>();
            if (k8sInfoBootstrap is null)
            {
                _logger.LogInformation("No service registered by type {0}. Either not running in a Kubernetes cluster or `{1}()` wasn't called on the service collection.", nameof(IK8sInfoBootstrap), nameof(AddApplicationInsightsKubernetesEnricher));
                return;
            }
            k8sInfoBootstrap.Run();
        }

        /// <summary>
        /// Checks if the KubernetesTelemetryInitializer exists in the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        private static bool KubernetesTelemetryInitializerExists(IServiceCollection serviceCollection)
            => serviceCollection.Any(t => !t.IsKeyedService && t.ImplementationType == typeof(KubernetesTelemetryInitializer));

        /// <summary>
        /// Configure the KubernetesTelemetryInitializer and its dependencies.
        /// </summary>
        internal static void ConfigureKubernetesTelemetryInitializer(
            this IServiceCollection services,
            Action<AppInsightsForKubernetesOptions>? overwriteOptions,
            IClusterEnvironmentCheck? clusterCheck,
            bool skipRegisterBackendService = false)
        {
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = new KubernetesServiceCollectionBuilder(overwriteOptions, clusterCheck, skipRegisterBackendService);
            _ = kubernetesServiceCollectionBuilder.RegisterServices(services);
        }
    }
}
