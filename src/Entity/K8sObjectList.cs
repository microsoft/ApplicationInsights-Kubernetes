namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sObjectList<T> : K8sObject
    {
        [JsonProperty("items")]
        public IEnumerable<T> Items { get; set; }
    }
}
