using System;
using System.IO;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.Logging;

using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extnesion method to inject Kubernetes Telemtry Initializer.
    /// </summary>
    public static class ApplicationInsightsExtensions
    {
        /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemtryConfiguration in the dependency injection system.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="timeout">Maximum time to wait for spinning up the container.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Collection builder.</param>
        /// <param name="detectKubernetes">Delegate to detect if the current application is running in Kubernetes hosted container.</param>
        /// <returns>The collection of services descriptors we injected into.</returns>
        public static IServiceCollection AddAppInsightsTelemetryKubernetesEnricher(
            this IServiceCollection services,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null)
        {
            // Inject of the service shall return immediately.
            EnableKubernetesImpl(services, detectKubernetes, kubernetesServiceCollectionBuilder, null, timeout);
            return services;
        }

        /// <summary>
        /// Enables Application Insights Kubernetes for a given TelemetryConfiguration.
        /// </summary>
        /// <remarks>
        /// The use of EnableKubernetes() on the ServiceCollection is always preferred unless you have more than one TelemetryConfiguration
        /// instance, or if you are using Application Insights from a non ASP.NET environment, like a console app.
        /// </remarks>
        public static void AddAppInsightsTelemetryKubernetesEnricher(
            this TelemetryConfiguration telemetryConfiguration,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null)
        {
            IServiceCollection standaloneServiceCollection = new ServiceCollection();
            standaloneServiceCollection = EnableKubernetesImpl(standaloneServiceCollection, detectKubernetes, kubernetesServiceCollectionBuilder, null, timeout);

            // Static class can't used as generic types.
            ILogger logger = standaloneServiceCollection.GetLogger<IKubernetesServiceCollectionBuilder>();
            IServiceProvider serviceProvider = standaloneServiceCollection.BuildServiceProvider();
            ITelemetryInitializer k8sTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>()
                .FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            if (k8sTelemetryInitializer != null)
            {
                telemetryConfiguration.TelemetryInitializers.Add(k8sTelemetryInitializer);
            }
            else
            {
                logger.LogError($"Getting ${nameof(KubernetesTelemetryInitializer)} from the service provider failed.");
            }
        }

        /// <summary>
        /// Please use AddAppInsightsTelemetryKubernetesEnricher() insead.
        /// Enables Application Insights Kubernetes for the Default TelemtryConfiguration in the dependency injection system.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="timeout">Maximum time to wait for spinning up the container.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Collection builder.</param>
        /// <param name="detectKubernetes">Delegate to detect if the current application is running in Kubernetes hosted container.</param>
        /// <returns>The collection of services descriptors we injected into.</returns>
        [Obsolete("Use AddAppInsightsTelemetryKubernetesEnricher() instead", false)]
        public static IServiceCollection EnableKubernetes(
            this IServiceCollection services,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null)
        {
            return services.AddAppInsightsTelemetryKubernetesEnricher(timeout, kubernetesServiceCollectionBuilder, detectKubernetes);
        }

        /// <summary>
        /// Please use "AddAppInsightsTelemetryKubernetesEnricher()" insead.
        /// Enables Application Insights Kubernetes for a given
        /// TelemetryConfiguration.
        /// </summary>
        /// <remarks>
        /// The use of EnableKubernetes() on the ServiceCollection is always
        /// preferred unless you have more than one TelemetryConfiguration
        /// instance, or if you are using Application Insights from a non ASP.NET
        /// environment, like a console app.
        /// </remarks>
        [Obsolete("Use AddAppInsightsTelemetryKubernetesEnricher() instead", false)]
        public static void EnableKubernetes(
            this TelemetryConfiguration telemetryConfiguration,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null)
        {
            telemetryConfiguration.AddAppInsightsTelemetryKubernetesEnricher(
                timeout, kubernetesServiceCollectionBuilder, detectKubernetes);
        }

        /// <summary>
        /// Enables applicaiton insights for kubernetes.
        /// </summary>
        private static IServiceCollection EnableKubernetesImpl(IServiceCollection serviceCollection,
            Func<bool> detectKubernetes,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder,
            ILogger<KubernetesServiceCollectionBuilder> logger = null,
            TimeSpan? timeout = null)
        {
            logger = logger ?? serviceCollection.GetLogger<KubernetesServiceCollectionBuilder>();

            // 2 minutes by default maximum to wait for spinning up the container.
            timeout = timeout ?? TimeSpan.FromMinutes(2);

            logger.LogInformation(Invariant($"ApplicationInsights.Kubernetes.Version:{SDKVersionUtils.Instance.CurrentSDKVersion}"));
            try
            {
                serviceCollection = BuildK8sServiceCollection(serviceCollection, timeout.Value, detectKubernetes, logger, kubernetesServiceCollectionBuilder);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to fetch ApplicaitonInsights.Kubernetes' info. Details " + ex.ToString());
            }
            return serviceCollection;
        }

        private static IServiceCollection BuildK8sServiceCollection(
            IServiceCollection services,
            TimeSpan timeout,
            Func<bool> detectKubernetes,
            ILogger<KubernetesServiceCollectionBuilder> logger,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            detectKubernetes = detectKubernetes ?? IsRunningInKubernetes;
            kubernetesServiceCollectionBuilder = kubernetesServiceCollectionBuilder ?? new KubernetesServiceCollectionBuilder(detectKubernetes, logger);
            services = kubernetesServiceCollectionBuilder.InjectServices(services, timeout);
            return services;
        }

        private static bool IsRunningInKubernetes() => Directory.Exists(@"/var/run/secrets/kubernetes.io") || Directory.Exists(@"C:\var\run\secrets\kubernetes.io");

        /// <summary>
        /// Gets a logger for given type.
        /// Note: This method leads to build service provider during the injection of services and shall be avoid whenever possible.
        /// </summary>
        private static ILogger<T> GetLogger<T>(this IServiceCollection services)
        {
            // AddLogging() is safe to call multiple times.
            // https://github.com/aspnet/Logging/blob/75a1cecf24f8418a45426b6cc3606f0d53640f89/src/Microsoft.Extensions.Logging/LoggingServiceCollectionExtensions.cs#L41
            return services.AddLogging().BuildServiceProvider().GetService<ILogger<T>>();
        }
    }
}
