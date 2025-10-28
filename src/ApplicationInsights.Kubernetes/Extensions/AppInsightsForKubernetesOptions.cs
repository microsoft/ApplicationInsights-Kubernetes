using System;
using Microsoft.ApplicationInsights.Kubernetes;

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
        /// Gets or sets the time-span for exponent interval base. This will be used as the interval between querying the Kubernetes cluster for properties.
        /// For example, in y = power(2, x), 2 is the base, and the interval will then be 2 seconds, 4 seconds, 8 seconds, 16 seconds ... until it reached the
        /// <see cref="ClusterInfoRefreshInterval" />.
        /// The base is default to 2 seconds.
        /// </summary>
        public TimeSpan ExponentIntervalBase { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Get or sets how frequent to refresh the cluster info.
        /// For example: 00:10:00 for 10 minutes.
        /// The default value is 10 minutes.
        /// </summary>
        public TimeSpan ClusterInfoRefreshInterval { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets or sets an environment check action to determine if the the current process is inside a Kubernetes cluster.
        /// When set to null (also the default), a built-in checker will be used.
        /// </summary>
        public IClusterEnvironmentCheck? ClusterCheckAction { get; set; } = null;

        /// <summary>
        /// For backward compatibility reason to allow the user to opt-in to 
        /// keep overwriting the SDK version in telemetry. It doesn't make too
        /// much sense to use it and has caused quite a confusion on the support
        /// side.
        /// Default to false and look into totally get rid of it in the future.
        /// </summary>
        public bool OverwriteSDKVersion { get; set; }

        /// <summary>
        /// Exclude node information from cluster info by skipping calls to the nodes endpoint.
        /// Default is false.
        /// </summary>
        public bool ExcludeNodeInformation { get; set; }
    }
}
