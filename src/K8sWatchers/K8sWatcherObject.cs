namespace Microsoft.ApplicationInsights.Kubernetes
{
    using Microsoft.ApplicationInsights.Kubernetes.Entities;
    using Newtonsoft.Json;

    /// <summary>
    /// Base class for object returned by K8s watch API.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sWatcherObject<TEntity, TMetadata>
        where TEntity : K8sEntity<TMetadata>
        where TMetadata : K8sObjectMetadata
    {
        /// <summary>
        /// Type of the event. For example, 'ADD'.
        /// </summary>
        [JsonProperty("type")]
        public string EventType { get; set; }

        /// <summary>
        /// Object returned by the watch API. For example, 'Pod'.
        /// </summary>
        [JsonProperty("object")]
        public TEntity EventObject { get; set; }
    }
}
