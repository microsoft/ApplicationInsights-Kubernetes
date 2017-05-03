namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System;
    using Newtonsoft.Json;
    internal class K8sEntity<TMetadata> : K8sObject
    {
        [JsonProperty("metadata")]
        public TMetadata Metadata { get; set; }
    }
}
