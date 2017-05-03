
namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sReplicaSet : K8sEntity<K8sReplicaSetMetadata>
    {
    }
}