namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class Selector
    {
        [JsonProperty("matchLabels")]
        public IDictionary<string, string> MatchLabels { get; set; }
    }
}
