namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]

    public class OwnerReference : K8sObject
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("uid")]
        public string Uid { get; set; }
    }
}
