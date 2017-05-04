
namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sReplicaSet : K8sEntity<K8sReplicaSetMetadata>
    {
    }
}