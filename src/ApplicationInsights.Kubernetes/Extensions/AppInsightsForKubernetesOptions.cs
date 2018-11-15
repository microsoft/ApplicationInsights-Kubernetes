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
        /// <value></value>
        [JsonProperty("InitializationTimeout")]
        public TimeSpan InitializationTimeout { get; set; } = TimeSpan.FromMinutes(2);
    }
}