namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sPod : K8sEntity<K8sPodMetadata>
    {
        [JsonProperty("status")]
        public K8sPodStatus Status { get; set; }

        [JsonProperty("spec")]
        public K8sPodSpec Spec { get; set; }
    }
}
