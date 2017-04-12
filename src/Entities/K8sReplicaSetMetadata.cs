using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sReplicaSetMetadata : K8sObjectMetadata<K8sReplicaSet>
    {
    }
}
