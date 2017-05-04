namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sPodList : K8sObjectList<K8sPod>
    {
    }
}
