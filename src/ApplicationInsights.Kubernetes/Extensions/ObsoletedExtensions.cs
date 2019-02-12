using System;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// This class holds all the obsoleted methods for ApplicationInsightsExtensions for readability.
    /// </summary>
    public static partial class ApplicationInsightsExtensions
    {
        /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemetryConfiguration in the dependency injection system.
        /// </summary>
        /// <param name="services">Sets the collection of service descriptors.</param>
        /// <param name="timeout">Sets the maximum time to wait for spinning up the container.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Sets the collection builder.</param>
        /// <param name="detectKubernetes">Sets a delegate to detect if the current application is running in Kubernetes hosted container.</param>
        /// <param name="logger">Sets a logger for building the service collection.</param>
        /// <returns>The collection of services descriptors injected into.</returns>
        [Obsolete("Use AddApplicationInsightsKubernetesEnricher with Options instead.", error: false)]
        public static IServiceCollection AddApplicationInsightsKubernetesEnricher(
            this IServiceCollection services,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null,
            ILogger<IKubernetesServiceCollectionBuilder> logger = null)
        {
            return services.AddApplicationInsightsKubernetesEnricher(option =>
            {
                if (timeout != null && timeout.HasValue)
                {
                    option.InitializationTimeout = timeout.Value;
                }
            }, kubernetesServiceCollectionBuilder, detectKubernetes, logger);
        }

        /// <summary>
        /// Enables Application Insights Kubernetes for a given TelemetryConfiguration.
        /// </summary>
        /// <remarks>
        /// The use of AddApplicationInsightsKubernetesEnricher() on the ServiceCollection is always preferred unless you have more than one TelemetryConfiguration
        /// instance, or if you are using Application Insights from a non ASP.NET environment, like a console app.
        /// </remarks>
        [Obsolete("Use AddApplicationInsightsKubernetesEnricher with Options instead.", error: false)]
        public static void AddApplicationInsightsKubernetesEnricher(
            this TelemetryConfiguration telemetryConfiguration,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null,
            ILogger<IKubernetesServiceCollectionBuilder> logger = null)
        {
            IServiceCollection standaloneServiceCollection = new ServiceCollection();
            standaloneServiceCollection = standaloneServiceCollection.AddApplicationInsightsKubernetesEnricher(
                timeout, kubernetesServiceCollectionBuilder, detectKubernetes);

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
        /// Please use AddApplicationInsightsKubernetesEnricher() instead.
        /// Enables Application Insights Kubernetes for the Default TelemetryConfiguration in the dependency injection system.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="timeout">Maximum time to wait for spinning up the container.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Collection builder.</param>
        /// <param name="detectKubernetes">Delegate to detect if the current application is running in Kubernetes hosted container.</param>
        /// <returns>The collection of services descriptors injected into.</returns>
        [Obsolete("Use AddApplicationInsightsKubernetesEnricher() instead", false)]
        public static IServiceCollection EnableKubernetes(
            this IServiceCollection services,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null)
        {
            return services.AddApplicationInsightsKubernetesEnricher(timeout, kubernetesServiceCollectionBuilder, detectKubernetes);
        }

        /// <summary>
        /// Please use "AddApplicationInsightsKubernetesEnricher()" instead.
        /// Enables Application Insights Kubernetes for a given
        /// TelemetryConfiguration.
        /// </summary>
        /// <remarks>
        /// The use of AddApplicationInsightsKubernetesEnricher() on the ServiceCollection is always
        /// preferred unless you have more than one TelemetryConfiguration
        /// instance, or if you are using Application Insights from a non ASP.NET
        /// environment, like a console app.
        /// </remarks>
        [Obsolete("Use AddApplicationInsightsKubernetesEnricher() instead", false)]
        public static void EnableKubernetes(
            this TelemetryConfiguration telemetryConfiguration,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null)
        {
            telemetryConfiguration.AddApplicationInsightsKubernetesEnricher(
                timeout, kubernetesServiceCollectionBuilder, detectKubernetes);
        }

    }
}