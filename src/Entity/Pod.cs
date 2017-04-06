namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Pod
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("metadata")]
        public PodMetadata Metadata { get; set; }

        [JsonProperty("status")]
        public PodStatus Status { get; set; }
    }
}
