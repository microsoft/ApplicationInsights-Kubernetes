using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{

    public class K8sObject
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }
    }
}
