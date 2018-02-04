
namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sReplicaSet : K8sObject
    {
        [JsonProperty("metadata")]
        public K8sReplicaSetMetadata Metadata { get; set; }
    }
}