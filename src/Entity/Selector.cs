namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Selector
    {
        [JsonProperty("matchLabels")]
        public IDictionary<string, string> MatchLabels { get; set; }
    }
}
