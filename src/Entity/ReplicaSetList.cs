namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ReplicaSetList : K8sObjectList<ReplicaSet>
    {
    }
}
