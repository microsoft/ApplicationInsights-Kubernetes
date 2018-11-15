using System;
using System.IO;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extnesion method to inject Kubernetes Telemtry Initializer.
    /// </summary>
    public static class ApplicationInsightsExtensions
    {
        private const string ConfigurationSectionName = "AppInsightsForKubernetes";

        /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemtryConfiguration in the dependency injection system.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="timeout">Maximum time to wait for spinning up the container.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Collection builder.</param>
        /// <param name="detectKubernetes">Delegate to detect if the current application is running in Kubernetes hosted container.</param>
        /// <param name="logger">Sets a logger for building the service collection.</param>
        /// <returns>The collection of services descriptors we injected into.</returns>
        public static IServiceCollection AddApplicationInsightsKubernetesEnricher(
            this IServiceCollection services,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null,
            ILogger<IKubernetesServiceCollectionBuilder> logger = null)
        {
            // Inject of the service shall return immediately.
            return EnableKubernetesImpl(services, detectKubernetes, kubernetesServiceCollectionBuilder, logger: logger, timeout: timeout);
        }

        /// <summary>
        /// Enables Application Insights Kubernetes for a given TelemetryConfiguration.
        /// </summary>
        /// <remarks>
        /// The use of AddApplicationInsightsKubernetesEnricher() on the ServiceCollection is always preferred unless you have more than one TelemetryConfiguration
        /// instance, or if you are using Application Insights from a non ASP.NET environment, like a console app.
        /// </remarks>
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
        /// Please use AddApplicationInsightsKubernetesEnricher() insead.
        /// Enables Application Insights Kubernetes for the Default TelemtryConfiguration in the dependency injection system.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="timeout">Maximum time to wait for spinning up the container.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Collection builder.</param>
        /// <param name="detectKubernetes">Delegate to detect if the current application is running in Kubernetes hosted container.</param>
        /// <returns>The collection of services descriptors we injected into.</returns>
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
        /// Please use "AddApplicationInsightsKubernetesEnricher()" insead.
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

        /// <summary>
        /// Enables applicaiton insights for kubernetes.
        /// </summary>
        private static IServiceCollection EnableKubernetesImpl(IServiceCollection serviceCollection,
            Func<bool> detectKubernetes,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder,
            TimeSpan? timeout = null,
            ILogger<IKubernetesServiceCollectionBuilder> logger = null)
        {
            BindOptions(serviceCollection, timeout);
            serviceCollection.AddLogging();
            serviceCollection = BuildK8sServiceCollection(
                serviceCollection,
                detectKubernetes,
                logger: logger,
                kubernetesServiceCollectionBuilder: kubernetesServiceCollectionBuilder);
            return serviceCollection;
        }

        private static void BindOptions(IServiceCollection serviceCollection, TimeSpan? timeout)
        {
            serviceCollection.AddOptions();
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            IOptions<AppInsightsForKubernetesOptions> options = serviceProvider.GetService<IOptions<AppInsightsForKubernetesOptions>>();

            if (options.Value == null)
            {
                serviceCollection.AddSingleton<IOptions<AppInsightsForKubernetesOptions>>(
                           new OptionsWrapper<AppInsightsForKubernetesOptions>(new AppInsightsForKubernetesOptions()));
            }

            // Update settings from configuration.
            IConfiguration configuration = serviceProvider.GetService<IConfiguration>();
            if (configuration != null)
            {
                serviceCollection.Configure<AppInsightsForKubernetesOptions>(configuration.GetSection(ConfigurationSectionName));
            }

            // Update settings when parameter is provided for backward compatibility.
            if (timeout != null)
            {
                serviceCollection.Configure<AppInsightsForKubernetesOptions>(option =>
                {
                    option.InitializationTimeout = timeout.Value;
                });
            }
        }

        private static IServiceCollection BuildK8sServiceCollection(
            IServiceCollection services,
            Func<bool> detectKubernetes,
            ILogger<IKubernetesServiceCollectionBuilder> logger,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            detectKubernetes = detectKubernetes ?? IsRunningInKubernetes;
            kubernetesServiceCollectionBuilder = kubernetesServiceCollectionBuilder ??
                new KubernetesServiceCollectionBuilder(detectKubernetes, logger);

            services = kubernetesServiceCollectionBuilder.InjectServices(services);

            return services;
        }

        private static bool IsRunningInKubernetes() => Directory.Exists(@"/var/run/secrets/kubernetes.io") || Directory.Exists(@"C:\var\run\secrets\kubernetes.io");
    }
}
