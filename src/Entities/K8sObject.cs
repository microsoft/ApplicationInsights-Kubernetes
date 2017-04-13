using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{

    public class K8sObject
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }
    }
}
