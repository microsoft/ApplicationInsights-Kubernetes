namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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
                        ApplicationInsightsExtensions.EnableK8s(loggerFactory, timeout);
                    }
                }
            }
        }
    }
}
