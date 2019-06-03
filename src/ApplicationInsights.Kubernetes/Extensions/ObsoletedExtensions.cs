using System;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// This class holds all the obsoleted methods for ApplicationInsightsExtensions for readability.
    /// This file is NOT built or maintained. It is here just for reference.
    /// </summary>
    public static partial class ApplicationInsightsExtensions
    {
        /// <summary>
        /// Enables Application Insights Kubernetes for a given TelemetryConfiguration.
        /// </summary>
        /// <param name="telemetryConfiguration">Sets the telemetry configuration to add the telemetry initializer to.</param>
        /// <param name="applyOptions">Sets a delegate to apply the configuration for the telemetry initializer.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Sets a service collection builder.</param>
        /// <param name="detectKubernetes">Sets a delegate to detect if the current application is running in Kubernetes hosted container.</param>
        /// <param name="logger">Sets a logger for building the service collection.</param>
        [Obsolete("ILogger is not used for Application Insights Kubernetes self diagnostics anymore. Use the other overloads.", true)]
        public static void AddApplicationInsightsKubernetesEnricher(
            this TelemetryConfiguration telemetryConfiguration,
            Action<AppInsightsForKubernetesOptions> applyOptions = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null,
            ILogger<IKubernetesServiceCollectionBuilder> logger = null)
        {
            IServiceCollection standaloneServiceCollection = new ServiceCollection();
            standaloneServiceCollection = standaloneServiceCollection.AddApplicationInsightsKubernetesEnricher(
                applyOptions,
                kubernetesServiceCollectionBuilder,
                detectKubernetes,
                logger);

            // Static class can't used as generic types.
            IServiceProvider serviceProvider = standaloneServiceCollection.BuildServiceProvider();
            ITelemetryInitializer k8sTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>()
                .FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            if (k8sTelemetryInitializer != null)
            {
                telemetryConfiguration.TelemetryInitializers.Add(k8sTelemetryInitializer);

                logger?.LogInformation($"{nameof(KubernetesTelemetryInitializer)} is injected.");
            }
            else
            {
                logger?.LogError($"Getting ${nameof(KubernetesTelemetryInitializer)} from the service provider failed.");
            }
        }

                /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemetryConfiguration in the dependency injection system with custom options and debugging components.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="applyOptions">Action to customize the configuration of Application Insights for Kubernetes.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Sets the service collection builder for Application Insights for Kubernetes to overwrite the default one.</param>
        /// <param name="detectKubernetes">Sets a delegate overwrite the default detector of the Kubernetes environment.</param>
        /// <param name="logger">Sets a logger to overwrite the default logger from the given service collection.</param>
        /// <returns>The collection of services descriptors injected into.</returns>
        [Obsolete("ILogger is not used for Application Insights Kubernetes self diagnostics anymore. Use the other overloads.", true)]
        public static IServiceCollection AddApplicationInsightsKubernetesEnricher(
            this IServiceCollection services,
            Action<AppInsightsForKubernetesOptions> applyOptions,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder,
            Func<bool> detectKubernetes,
            ILogger<IKubernetesServiceCollectionBuilder> logger)
        {
            throw new NotImplementedException();
        }
    }
}