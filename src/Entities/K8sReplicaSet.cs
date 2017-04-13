using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]

    public class K8sReplicaSet : K8sObject
    {
        [JsonProperty("metadata")]
        public K8sReplicaSetMetadata Metadata { get; set; }
    }
}