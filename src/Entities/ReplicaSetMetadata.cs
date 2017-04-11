using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ReplicaSetMetadata : K8sObjectMetadata<ReplicaSet>
    {
    }
}
