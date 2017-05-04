using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sError : K8sEntity<K8sErrorMetadata>
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
