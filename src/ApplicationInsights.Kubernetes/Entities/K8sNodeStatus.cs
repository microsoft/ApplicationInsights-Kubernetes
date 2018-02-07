namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sNodeStatus
    {
        [JsonProperty("images")]
        public IEnumerable<K8sNodeImage> Images { get; set; }
    }
}