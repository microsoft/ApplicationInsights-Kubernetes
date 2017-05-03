namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sDeploymentMetadata : K8sObjectMetadata
    {

    }
}
