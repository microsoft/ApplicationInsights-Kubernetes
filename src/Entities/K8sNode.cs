namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sNode : K8sEntity<K8sNodeMetadata>
    {
        [JsonProperty("status")]
        public K8sNodeStatus Status { get; set; }
    }
}