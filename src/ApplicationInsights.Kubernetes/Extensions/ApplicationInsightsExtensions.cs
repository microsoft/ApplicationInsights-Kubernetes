using System;
using System.IO;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
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
        private const string ConfigurationSectionName = "AppInsightsForKubernetes";
        private static readonly Logger _diagnosticSource = Logger.Instance;

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
            return services.Configure<TelemetryConfiguration>((config) =>
            {
                KubernetesTelemetryInitializer k8sTelemetryInitializer;
                if (!KubernetesTelemetryInitializerExists(services))
                {
                    InjectKubernetesTelemetryInitializer(services, detectKubernetes, kubernetesServiceCollectionBuilder, applyOptions);
                }
                k8sTelemetryInitializer = GetKubernetesTelemetryInitializer(services);
                config.AddKubernetesTelemetryInitializer(k8sTelemetryInitializer);
            });
        }

        /// <summary>
        /// Enables Application Insights Kubernetes for a given TelemetryConfiguration.
        /// </summary>
        /// <param name="telemetryConfiguration">Sets the telemetry configuration to add the telemetry initializer to.</param>
        /// <param name="applyOptions">Sets a delegate to apply the configuration for the telemetry initializer.</param>
        /// <param name="kubernetesServiceCollectionBuilder">Sets a service collection builder.</param>
        /// <param name="detectKubernetes">Sets a delegate to detect if the current application is running in Kubernetes hosted container.</param>
        public static void AddApplicationInsightsKubernetesEnricher(
            this TelemetryConfiguration telemetryConfiguration,
            Action<AppInsightsForKubernetesOptions> applyOptions = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null,
            Func<bool> detectKubernetes = null)
        {
            IServiceCollection standaloneServiceCollection = new ServiceCollection();
            InjectKubernetesTelemetryInitializer(standaloneServiceCollection, detectKubernetes, kubernetesServiceCollectionBuilder, applyOptions);
            telemetryConfiguration.AddKubernetesTelemetryInitializer(GetKubernetesTelemetryInitializer(standaloneServiceCollection));
        }

        /// <summary>
        /// Adds a KubernetesTelemetry into a TelemetryConfiguration instance.
        /// </summary>
        private static void AddKubernetesTelemetryInitializer(
            this TelemetryConfiguration telemetryConfiguration,
            KubernetesTelemetryInitializer k8sTelemetryInitializer)
        {
            if (k8sTelemetryInitializer != null)
            {
                telemetryConfiguration.TelemetryInitializers.Add(k8sTelemetryInitializer);
                _diagnosticSource.LogTrace("KubernetesTelemetryInitializer has been injected into telemetry configuration #{0}.", telemetryConfiguration.GetHashCode());
                _diagnosticSource.LogInformation("KubernetesTelemetryInitializer is injected.");
            }
            else
            {
                _diagnosticSource.LogError("No KubernetesTelemetryInitializer to append to TelemetryConfiguration.");
            }
        }

        /// <summary>
        /// Checks if the KubernetesTelemetryInitializer exists in the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        private static bool KubernetesTelemetryInitializerExists(IServiceCollection serviceCollection)
        {
            return serviceCollection.Any<ServiceDescriptor>(t => t.ImplementationType == typeof(KubernetesTelemetryInitializer));
        }

        /// <summary>
        /// Gets the KubernetesTelemetryInitializer from a service collection when it exists.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>Returns the instance of the KubernetesTelemetryInitializer when it exists. Null when it doesn't.</returns>
        private static KubernetesTelemetryInitializer GetKubernetesTelemetryInitializer(IServiceCollection serviceCollection)
        {
            if (KubernetesTelemetryInitializerExists(serviceCollection))
            {
                IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
                return serviceProvider.GetServices<ITelemetryInitializer>()
                    .FirstOrDefault(ti => ti.GetType() == typeof(KubernetesTelemetryInitializer)) as KubernetesTelemetryInitializer;
            }
            else
            {
                _diagnosticSource.LogTrace("KubernetesTelemetryInitializer doesn't exist in the service collection.");
                return null;
            }
        }

        /// <summary>
        /// Injects the KubernetesTelemetryInitializer to the service collection.
        /// </summary>
        internal static void InjectKubernetesTelemetryInitializer(IServiceCollection serviceCollection,
            Func<bool> detectKubernetes,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder,
            Action<AppInsightsForKubernetesOptions> applyOptions)
        {
            BuildServiceBases(serviceCollection, applyOptions);
            BuildK8sServiceCollection(
                serviceCollection,
                detectKubernetes,
                kubernetesServiceCollectionBuilder: kubernetesServiceCollectionBuilder);
        }

        private static void BuildServiceBases(
            IServiceCollection serviceCollection,
            Action<AppInsightsForKubernetesOptions> applyOptions)
        {
            serviceCollection.AddOptions();
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Apply the configurations if not yet.
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
            if (applyOptions != null)
            {
                serviceCollection.Configure<AppInsightsForKubernetesOptions>(option =>
                {
                    applyOptions(option);
                });
            }
        }

        private static void BuildK8sServiceCollection(
            IServiceCollection services,
            Func<bool> detectKubernetes,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            detectKubernetes = detectKubernetes ?? IsRunningInKubernetes;
            kubernetesServiceCollectionBuilder = kubernetesServiceCollectionBuilder ??
                new KubernetesServiceCollectionBuilder(detectKubernetes);

            kubernetesServiceCollectionBuilder.InjectServices(services);
        }

        private static bool IsRunningInKubernetes() => Directory.Exists(@"/var/run/secrets/kubernetes.io") || Directory.Exists(@"C:\var\run\secrets\kubernetes.io");
    }
}
