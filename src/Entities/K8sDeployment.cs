namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sDeployment 
    {
        [JsonProperty("metadata")]
        public K8sDeploymentMetadata Metadata { get; set; }

        [JsonProperty("spec")]
        public K8sDeploymentSpec Spec { get; set; }
    }
}
