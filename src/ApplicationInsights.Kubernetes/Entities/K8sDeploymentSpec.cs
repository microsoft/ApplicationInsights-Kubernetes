namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sDeploymentSpec
    {
        [JsonProperty("selector")]
        public Selector Selector { get; set; }
    }
}
