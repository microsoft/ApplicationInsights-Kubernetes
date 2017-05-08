namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sPod : K8sObject
    {
        [JsonProperty("metadata")]
        public K8sPodMetadata Metadata { get; set; }

        [JsonProperty("status")]
        public K8sPodStatus Status { get; set; }

        [JsonProperty("spec")]
        public K8sPodSpec Spec { get; set; }
    }
}
