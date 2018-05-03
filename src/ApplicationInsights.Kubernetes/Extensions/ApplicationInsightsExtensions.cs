using System;
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
        /// Enable Application Insights Kubernetes for the Default TelemtryConfiguration in the dependency injection system.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="timeout"></param>
        /// <param name="kubernetesServiceCollectionBuilder"></param>
        /// <returns></returns>
        public static IServiceCollection EnableKubernetes(
            this IServiceCollection services,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            // Inject of the service shall return immediately.
            EnableKubernetesImpl(services, timeout, kubernetesServiceCollectionBuilder);
            return services;
        }

        /// <summary>
        /// Enable Application Insights Kubernetes for a given TelemetryConfiguration.
        /// Note: The use of EnableKubernetes() on the ServiceCollection is always preferred unless you have more than one
        /// TelemetryConfiguration instances.
        /// </summary>
        public static void EnableKubernetes(
            this TelemetryConfiguration telemetryConfiguration,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            if (telemetryConfiguration == null)
            {
                return;
            }

            IServiceCollection standaloneServiceCollection = new ServiceCollection();
            standaloneServiceCollection = EnableKubernetesImpl(standaloneServiceCollection, timeout, kubernetesServiceCollectionBuilder);

            IServiceProvider serviceProvider = standaloneServiceCollection.BuildServiceProvider();

            ITelemetryInitializer k8sTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>()
                .First(ti => ti.GetType() == typeof(KubernetesTelemetryInitializer));

            telemetryConfiguration.TelemetryInitializers.Add(k8sTelemetryInitializer);
        }

        /// <summary>
        /// Enable applicaiton insights for kubernetes.
        /// </summary>
        private static IServiceCollection EnableKubernetesImpl(IServiceCollection serviceCollection,
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            // 2 minutes by default maximum to wait for spinning up the container.
            timeout = timeout ?? TimeSpan.FromMinutes(2);

            // According to the code, adding logging will not overwrite existing logging classes
            // https://github.com/aspnet/Logging/blob/c821494678a30c323174bea8056f43b93a3ca6f4/src/Microsoft.Extensions.Logging/LoggingServiceCollectionExtensions.cs
            // Becuase it uses 'TryAdd()' extenion method on service collection.
            serviceCollection.AddLogging();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            ILogger logger = serviceProvider.GetService<ILogger<IServiceCollection>>();
            logger.LogInformation(Invariant($"ApplicationInsights.Kubernetes.Version:{SDKVersionUtils.Instance.CurrentSDKVersion}"));
            try
            {
                serviceCollection = BuildK8sServiceCollection(serviceCollection, timeout.Value, kubernetesServiceCollectionBuilder);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to fetch ApplicaitonInsights.Kubernetes' version info. Details" + ex.ToString());
            }
            return serviceCollection;
        }

        private static IServiceCollection BuildK8sServiceCollection(
            IServiceCollection services,
            TimeSpan timeout,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            kubernetesServiceCollectionBuilder = kubernetesServiceCollectionBuilder ?? new KubernetesServiceCollectionBuilder();
            services = kubernetesServiceCollectionBuilder.InjectServices(services, timeout);
            return services;
        }
    }
}
