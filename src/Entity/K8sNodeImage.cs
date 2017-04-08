namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sNodeImage
    {
        [JsonProperty("names")]
        public IList<string> Names { get; set; }

        [JsonProperty("sizeBytes")]
        public long SizeBytes { get; set; }
    }
}