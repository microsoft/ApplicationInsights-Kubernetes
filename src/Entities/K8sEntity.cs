namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;
    public abstract class K8sEntity<TMetadata> : K8sObject
    {
        [JsonProperty("metadata")]
        public TMetadata Metadata { get; set; }
    }
}
