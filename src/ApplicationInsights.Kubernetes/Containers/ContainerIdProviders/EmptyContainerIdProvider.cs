namespace Microsoft.ApplicationInsights.Kubernetes.Containers
{
    internal class EmptyContainerIdProvider : IContainerIdProvider
    {
        /// <summary>
        /// Gets string.Empty for container id. This is useful in environment like Windows, where there is no way to findout the container id.
        /// </summary>
        /// <returns>Returns true with output container id of null.</returns>
        public bool TryGetMyContainerId(out string? containerId)
        {
            // Notice that empty is a valid result, returns true; null would lead to false.
            containerId = string.Empty;
            return true;
        }
    }
}
