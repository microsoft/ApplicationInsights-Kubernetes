namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sDeploymentSpec
    {
        [JsonProperty("selector")]
        public Selector Selector { get; set; }
    }
}
