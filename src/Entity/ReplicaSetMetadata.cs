using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ReplicaSetMetadata : K8sObjectMetadata<ReplicaSet>
    {
    }
}
