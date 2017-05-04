namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sDeployment : K8sEntity<K8sDeploymentMetadata>
    {
        [JsonProperty("spec")]
        public K8sDeploymentSpec Spec { get; set; }
    }
}
