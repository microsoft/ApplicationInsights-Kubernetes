namespace Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PodStatus
    {
        [JsonProperty("phase")]
        public string Phase { get; set; }

        [JsonProperty("containerStatuses")]
        public IEnumerable<ContainerStatus> ContainerStatuses { get; set; }
    }
}
