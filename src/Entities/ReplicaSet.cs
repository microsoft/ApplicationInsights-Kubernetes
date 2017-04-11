using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]

    public class ReplicaSet : K8sObject
    {
        [JsonProperty("metadata")]
        public ReplicaSetMetadata Metadata { get; set; }
    }
}