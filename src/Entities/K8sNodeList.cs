namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sNodeList : K8sObjectList<K8sNode>
    {
    }
}
