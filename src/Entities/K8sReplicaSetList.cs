namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sReplicaSetList : K8sObjectList<K8sReplicaSet>
    {
    }
}
