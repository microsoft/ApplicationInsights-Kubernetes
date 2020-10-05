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
        /// Maximum time to wait for spinning up the container.
        /// </summary>
        [JsonProperty("InitializationTimeout")]
        public TimeSpan InitializationTimeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets to disable CPU and memory counters on telemetry.
        /// Optional. Default to false.
        /// </summary>
        [JsonProperty("DisableCounters")]
        public bool DisableCounters { get; set; }
    }
}