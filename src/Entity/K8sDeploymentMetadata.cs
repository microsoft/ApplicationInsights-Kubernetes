namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sDeploymentMetadata : K8sObjectMetadata<K8sDeployment>
    {

    }
}
