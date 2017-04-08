namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sDeployment 
    {
        [JsonProperty("metadata")]
        public K8sDeploymentMetadata Metadata { get; set; }

        [JsonProperty("spec")]
        public K8sDeploymentSpec Spec { get; set; }
    }
}
