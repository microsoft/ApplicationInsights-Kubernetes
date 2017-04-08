namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sNodeList : K8sObjectList<K8sNode>
    {
    }
}
