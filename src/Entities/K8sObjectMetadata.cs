namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]

    public abstract class K8sObjectMetadata
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("labels")]
        public IDictionary<string, string> Labels { get; set; }

        [JsonProperty("resourceVersion")]
        public string ResourceVersion { get; set; }
    }
}
