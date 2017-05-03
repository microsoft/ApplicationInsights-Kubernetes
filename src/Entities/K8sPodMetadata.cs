namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sPodMetadata : K8sObjectMetadata
    {
        [JsonProperty("ownerReferences")]
        public IEnumerable<OwnerReference> OwnerReferences { get; set; }
    }
}
