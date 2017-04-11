namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ContainerStatus
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("imageID")]
        public string ImageID { get; set; }

        [JsonProperty("containerID")]
        public string ContainerID { get; set; }

        [JsonProperty("ready")]
        public bool Ready { get; set; }
    }
}
