namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PodMetadata : K8sObjectMetadata<Pod>
    {
        [JsonProperty("ownerReferences")]
        public IEnumerable<OwnerReference> OwnerReferences { get; set; }
    }
}
