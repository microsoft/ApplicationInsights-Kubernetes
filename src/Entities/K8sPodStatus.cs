namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class K8sPodStatus
    {
        [JsonProperty("phase")]
        public string Phase { get; set; }

        [JsonProperty("containerStatuses")]
        public IEnumerable<ContainerStatus> ContainerStatuses { get; set; }
    }
}
