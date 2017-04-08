namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sPodSpec
    {
        [JsonProperty("nodeName")]
        public string NodeName { get; set; }
    }
}
