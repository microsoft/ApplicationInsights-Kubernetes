using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class KubernetesModule : ITelemetryModule
    {
        private static readonly object lockObject = new object();
        private static bool isInitialized = false;
        private static IServiceCollection _serviceCollection = null;

        /// <summary>
        /// Initialize KubernetesModule
        /// </summary>
        /// <param name="configuration"></param>
        public void Initialize(TelemetryConfiguration configuration)
        {
            Initialize(configuration, null);
        }

        /// <summary>
        /// Initialize KubernetesModule
        /// </summary>
        /// <param name="configuration">Telemetry configuration.</param>
        /// <param name="loggerFactory">Optional logger factory for self-diagnostics.</param>
        /// <param name="timeout">Timeout for creating the kubernetes environments.</param>
        public static void Initialize(TelemetryConfiguration configuration, TimeSpan? timeout)
        {
            // Temporary fix to make sure that we initialize module once.
            // It should be removed when configuration reading logic is moved to Web SDK.
            if (!isInitialized)
            {
                lock (lockObject)
                {
                    if (!isInitialized)
                    {
                        EnableKubernetes(null, configuration, timeout);
                    }
                }
            }
        }

        /// <summary>
        /// Enable applicaiton insights for kubernetes.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="timeout"></param>
        public static void EnableKubernetes(IServiceCollection serviceCollection, TelemetryConfiguration configuration, TimeSpan? timeout = null)
        {
            // 2 minutes maximum to spin up the container.
            timeout = timeout ?? TimeSpan.FromMinutes(2);

            serviceCollection = BuildK8sServiceCollection(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            ILogger logger = serviceProvider.GetService<ILogger<KubernetesModule>>();

            Task.Run(() =>
            {
                try
                {
                    string versionInfo = typeof(ApplicationInsightsExtensions).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                    logger.LogInformation(Invariant($"ApplicationInsights.Kubernetes.Version:{versionInfo}"));
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to fetch ApplicaitonInsights.Kubernetes' version info. Details" + ex.ToString());
                }
            });

            try
            {
                K8sEnvironment k8sEnv = serviceProvider.GetRequiredService<K8sEnvironmentFactory>().CreateAsync(timeout.Value).ConfigureAwait(false).GetAwaiter().GetResult();
                if (k8sEnv != null)
                {
                    // Inject the telemetry initializer.
                    ITelemetryInitializer initializer = new KubernetesTelemetryInitializer(k8sEnv, serviceProvider.GetService<ILogger<KubernetesTelemetryInitializer>>());
                    configuration.TelemetryInitializers.Add(initializer);
                    logger?.LogDebug("Application Insights Kubernetes injected the service successfully.");
                }
                else
                {
                    logger?.LogError("Application Insights Kubernetes failed to start.");
                }
                isInitialized = true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
            }
        }

        private static IServiceCollection BuildK8sServiceCollection(IServiceCollection original)
        {
            if (_serviceCollection == null || _serviceCollection != original)
            {
                _serviceCollection = original ?? new ServiceCollection();
                // According github code, adding logging will not overwrite existing logging classes
                // https://github.com/aspnet/Logging/blob/c821494678a30c323174bea8056f43b93a3ca6f4/src/Microsoft.Extensions.Logging/LoggingServiceCollectionExtensions.cs
                // Becuase it uses 'TryAdd()' extenion method on service collection.
                _serviceCollection.AddLogging();

                _serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider>(p => new KubeHttpClientSettingsProvider(logger: p.GetService<ILogger<KubeHttpClientSettingsProvider>>()));
                _serviceCollection.AddSingleton<KubeHttpClientFactory>();
                _serviceCollection.AddSingleton<K8sQueryClientFactory>();

                _serviceCollection.AddSingleton<K8sEnvironmentFactory>();
            }

            return _serviceCollection;
        }
    }
}
