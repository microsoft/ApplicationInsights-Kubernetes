namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sNode
    {
        [JsonProperty("metadata")]
        public K8sNodeMetadata Metadata { get; set; }

        [JsonProperty("status")]
        public K8sNodeStatus Status { get; set; }
    }
}