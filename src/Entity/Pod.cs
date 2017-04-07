namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Pod : K8sObject
    {
        [JsonProperty("metadata")]
        public PodMetadata Metadata { get; set; }

        [JsonProperty("status")]
        public PodStatus Status { get; set; }
    }
}
