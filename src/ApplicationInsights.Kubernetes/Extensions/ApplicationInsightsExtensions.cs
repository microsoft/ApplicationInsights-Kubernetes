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
        /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemetryConfiguration in the dependency injection container with custom options.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="applyOptions">Action to customize the configuration of Application Insights for Kubernetes.</param>
        /// <param name="diagnosticLogLevel">Sets the diagnostics log levels for the enricher.</param>
        /// <returns>The service collection for chaining the next operation.</returns>
        public static IServiceCollection AddApplicationInsightsKubernetesEnricher(
            this IServiceCollection services,
            Action<AppInsightsForKubernetesOptions>? applyOptions = null,
            LogLevel? diagnosticLogLevel = LogLevel.None)
        {
            diagnosticLogLevel ??= LogLevel.None;   // Default to None.
            if (diagnosticLogLevel != LogLevel.None)
            {
                ApplicationInsightsKubernetesDiagnosticObserver observer = new ApplicationInsightsKubernetesDiagnosticObserver((DiagnosticLogLevel)diagnosticLogLevel);
                ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);
            }

            if (!KubernetesTelemetryInitializerExists(services))
            {
                services.ConfigureKubernetesTelemetryInitializer(applyOptions, clusterCheck: default);
            }
            return services;
        }

        /// <summary>
        /// Checks if the KubernetesTelemetryInitializer exists in the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        private static bool KubernetesTelemetryInitializerExists(IServiceCollection serviceCollection)
            => serviceCollection.Any<ServiceDescriptor>(t => t.ImplementationType == typeof(KubernetesTelemetryInitializer));

        /// <summary>
        /// Configure the KubernetesTelemetryInitializer and its dependencies.
        /// </summary>
        internal static void ConfigureKubernetesTelemetryInitializer(
            this IServiceCollection services,
            Action<AppInsightsForKubernetesOptions>? overwriteOptions,
            IClusterEnvironmentCheck? clusterCheck)
        {
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = new KubernetesServiceCollectionBuilder(overwriteOptions, clusterCheck);
            kubernetesServiceCollectionBuilder.RegisterServices(services);
        }
    }
}
