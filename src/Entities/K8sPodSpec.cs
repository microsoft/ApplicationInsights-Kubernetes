namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sPodSpec
    {
        [JsonProperty("nodeName")]
        public string NodeName { get; set; }
    }
}
