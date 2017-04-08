namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sPodMetadata : K8sObjectMetadata<K8sPod>
    {
        [JsonProperty("ownerReferences")]
        public IEnumerable<OwnerReference> OwnerReferences { get; set; }
    }
}
