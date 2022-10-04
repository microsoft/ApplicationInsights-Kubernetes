using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Object model of configuration for Application Insights for Kubernetes.
    /// </summary>
    public class AppInsightsForKubernetesOptions
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SectionName = "AppInsightsForKubernetes";

        /// <summary>
        /// Maximum time to wait for spinning up the container.
        /// </summary>
        public TimeSpan InitializationTimeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets the processor for telemetry key. This is introduced to allow customization of
        /// telemetry keys.
        /// </summary>
        public Func<string, string>? TelemetryKeyProcessor { get; set; }

        /// <summary>
        /// Get or sets how frequent to refresh the cluster info.
        /// For example: 00:10:00 for 10 minutes.
        /// The default value is 10 minutes.
        /// </summary>
        public TimeSpan ClusterInfoRefreshInterval { get; set; } = TimeSpan.FromMinutes(10);
    }
}
