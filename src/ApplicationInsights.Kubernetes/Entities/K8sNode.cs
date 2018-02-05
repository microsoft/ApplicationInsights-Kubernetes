namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sNode
    {
        [JsonProperty("metadata")]
        public K8sNodeMetadata Metadata { get; set; }

        [JsonProperty("status")]
        public K8sNodeStatus Status { get; set; }
    }
}