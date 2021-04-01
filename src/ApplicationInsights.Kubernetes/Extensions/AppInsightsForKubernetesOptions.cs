using System;
using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Object model of configuration for Application Insights for Kubernetes.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AppInsightsForKubernetesOptions
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SectionName = "AppInsightsForKubernetes";

        /// <summary>
        /// Maximum time to wait for spinning up the container.
        /// </summary>
        [JsonProperty("InitializationTimeout")]
        public TimeSpan InitializationTimeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets to disable CPU and memory counters on telemetry.
        /// Optional. Default to false.
        /// </summary>
        [JsonProperty("DisablePerformanceCounters")]
        public bool DisablePerformanceCounters { get; set; }

        /// <summary>
        /// Gets or sets the processor for telemetry key. This is introduced to allow customization of
        /// telemetry keys.
        /// </summary>
        public Func<string, string> TelemetryKeyProcessor { get; set; }
    }
}