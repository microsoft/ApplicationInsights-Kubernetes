namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]

    public class K8sObjectMetadata<TParent>
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("labels")]
        public IDictionary<string, string> Labels { get; set; }
    }
}
