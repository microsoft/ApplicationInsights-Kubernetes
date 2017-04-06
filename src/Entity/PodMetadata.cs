namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PodMetadata
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("labels")]
        public IDictionary<string, string> Labels { get; set; }
    }
}
