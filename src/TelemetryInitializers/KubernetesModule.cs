namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

    public class KubernetesModule : ITelemetryModule
    {
        private static readonly object lockObject = new object();
        private static bool isInitialized = false;

        /// <summary>
        /// Initialize KubernetesModule
        /// </summary>
        /// <param name="configuration"></param>
        public void Initialize(TelemetryConfiguration configuration)
        {
            Initialize(configuration, null, null);
        }

        /// <summary>
        /// Initialize KubernetesModule
        /// </summary>
        /// <param name="configuration">Telemetry configuration.</param>
        /// <param name="loggerFactory">Optional logger factory for self-diagnostics.</param>
        /// <param name="timeout">Timeout for creating the kubernetes environments.</param>
        public void Initialize(TelemetryConfiguration configuration, ILoggerFactory loggerFactory, TimeSpan? timeout)
        {
            // Temporary fix to make sure that we initialize module once.
            // It should be removed when configuration reading logic is moved to Web SDK.
            if (!isInitialized)
            {
                lock (lockObject)
                {
                    if (!isInitialized)
                    {
                        EnableKubernetes(configuration, loggerFactory, timeout);
                    }
                }
            }
        }

        /// <summary>
        /// Enable applicaiton insights for kubernetes.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="timeout"></param>
        public static void EnableKubernetes(TelemetryConfiguration configuration, ILoggerFactory loggerFactory = null, TimeSpan? timeout = null)
        {
            // 2 minutes maximum to spin up the container.
            timeout = timeout ?? TimeSpan.FromMinutes(2);

            ILogger logger = loggerFactory?.CreateLogger("K8sEnvInitializer");

            Task.Run(() =>
            {
                try
                {
                    string versionInfo = typeof(ApplicationInsightsExtensions).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                    logger?.LogInformation(Invariant($"ApplicationInsights.Kubernetes.Version:{versionInfo}"));
                }
                catch (Exception ex)
                {
                    logger?.LogError("Failed to fetch ApplicaitonInsights.Kubernetes' version info. Details" + ex.ToString());
                }
            });

            try
            {
                K8sEnvironment k8sEnv = K8sEnvironment.CreateAsync(timeout.Value, loggerFactory).ConfigureAwait(false).GetAwaiter().GetResult();
                if (k8sEnv != null)
                {
                    // Wait until the initialization is done.
                    TimeSpan maxWait = timeout.Value + TimeSpan.FromSeconds(30);
                    k8sEnv.InitializationWaiter.WaitOne(maxWait);

                    // Inject the telemetry initializer.
                    ITelemetryInitializer initializer = new KubernetesTelemetryInitializer(loggerFactory, k8sEnv);
                    configuration.TelemetryInitializers.Add(initializer);
                    logger?.LogDebug("Application Insights Kubernetes injected the service successfully.");
                }
                else
                {
                    logger?.LogError("Application Insights Kubernetes failed to start.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
            }
        }
    }
}
