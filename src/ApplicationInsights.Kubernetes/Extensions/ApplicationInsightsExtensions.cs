using System;
using System.IO;
using System.Linq;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension method to inject Kubernetes Telemetry Initializer.
    /// </summary>
    public static partial class ApplicationInsightsExtensions
    {
        /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemetryConfiguration in the dependency injection system.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <returns>The service collection for chaining the next operation.</returns>
        public static IServiceCollection AddApplicationInsightsKubernetesEnricher(
            this IServiceCollection services)
        {
            return AddApplicationInsightsKubernetesEnricher(services, applyOptions: null);
        }

        /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemetryConfiguration in the dependency injection system with custom options.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="applyOptions">Action to customize the configuration of Application Insights for Kubernetes.</param>
        /// <returns>The service collection for chaining the next operation.</returns>
        public static IServiceCollection AddApplicationInsightsKubernetesEnricher(
            this IServiceCollection services,
            Action<AppInsightsForKubernetesOptions> applyOptions)
        {
            return services.AddApplicationInsightsKubernetesEnricher(
                applyOptions,
                kubernetesServiceCollectionBuilder: null,
                detectKubernetes: null
            );
        }

        /// <summary>
        /// Enables Application Insights for Kubernetes on the Default TelemetryConfiguration in the dependency injection system with custom options and debugging components.
        /// </summary>
        /// <param name="services">Collection of service descriptors.</param>
        /// <param name="applyOptions">Action to customize the configuration of Application Insights for Kubernetes.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Sets the service collection builder for Application Insights for Kubernetes to overwrite the default one.</param>
        /// <param name="detectKubernetes">Sets a delegate overwrite the default detector of the Kubernetes environment.</param>
        /// <returns>The service collection for chaining the next operation.</returns>
        public static IServiceCollection AddApplicationInsightsKubernetesEnricher(
            this IServiceCollection services,
            Action<AppInsightsForKubernetesOptions> applyOptions,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder,
            Func<bool> detectKubernetes)
        {
            if (!KubernetesTelemetryInitializerExists(services))
            {
                ConfigureKubernetesTelemetryInitializer(services, detectKubernetes, kubernetesServiceCollectionBuilder, applyOptions);
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
        internal static void ConfigureKubernetesTelemetryInitializer(IServiceCollection serviceCollection,
            Func<bool> detectKubernetes,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder,
            Action<AppInsightsForKubernetesOptions> applyOptions)
        {
            InitializeServiceCollection(serviceCollection, applyOptions);
            BuildK8sServiceCollection(
                serviceCollection,
                detectKubernetes,
                kubernetesServiceCollectionBuilder: kubernetesServiceCollectionBuilder);
        }

        private static void InitializeServiceCollection(
            IServiceCollection serviceCollection,
            Action<AppInsightsForKubernetesOptions> applyOptions)
        {
            serviceCollection.AddOptions();
            serviceCollection.AddOptions<AppInsightsForKubernetesOptions>().Configure<IConfiguration>((opt, configuration) =>
            {
                configuration.GetSection(AppInsightsForKubernetesOptions.SectionName).Bind(opt);
                applyOptions?.Invoke(opt);
            });
        }

        private static void BuildK8sServiceCollection(
            IServiceCollection services,
            Func<bool> detectKubernetes,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            detectKubernetes = detectKubernetes ?? IsRunningInKubernetes;
            using IServiceScope serviceScope = services.BuildServiceProvider().CreateScope();
            var options = serviceScope.ServiceProvider.GetRequiredService<IOptions<AppInsightsForKubernetesOptions>>();
            kubernetesServiceCollectionBuilder = kubernetesServiceCollectionBuilder ?? new KubernetesServiceCollectionBuilder(detectKubernetes, options);
            kubernetesServiceCollectionBuilder.RegisterServices(services);
        }

        private static bool IsRunningInKubernetes() => Directory.Exists(@"/var/run/secrets/kubernetes.io") || Directory.Exists(@"C:\var\run\secrets\kubernetes.io");
    }
}
