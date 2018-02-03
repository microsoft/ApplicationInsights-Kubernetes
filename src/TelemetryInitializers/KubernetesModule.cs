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
                        EnableKubernetes(configuration, timeout);
                    }
                }
            }
        }

        /// <summary>
        /// Enable applicaiton insights for kubernetes.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="timeout"></param>
        public static void EnableKubernetes(TelemetryConfiguration configuration, TimeSpan? timeout = null)
        {
            // 2 minutes maximum to spin up the container.
            timeout = timeout ?? TimeSpan.FromMinutes(2);

            IServiceCollection serviceCollection = BuildK8sServiceCollection();
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
                    ITelemetryInitializer initializer = serviceProvider.GetService<KubernetesTelemetryInitializer>();
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

        private static IServiceCollection BuildK8sServiceCollection()
        {
            if (_serviceCollection == null)
            {
                _serviceCollection = new ServiceCollection();
                _serviceCollection.AddLogging();

                _serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider>(p => new KubeHttpClientSettingsProvider(logger: p.GetService<ILogger<KubeHttpClientSettingsProvider>>()));
                _serviceCollection.AddSingleton<KubeHttpClientFactory>();
                _serviceCollection.AddSingleton<K8sQueryClientFactory>();

                _serviceCollection.AddSingleton<K8sEnvironmentFactory>();
                _serviceCollection.AddTransient<KubernetesTelemetryInitializer>();
            }

            return _serviceCollection;
        }
    }
}
