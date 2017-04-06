namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PodList
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("items")]
        public IEnumerable<Pod> Items { get; set; }
    }
}
