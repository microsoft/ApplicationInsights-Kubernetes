#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders
{
    internal class NullContainerIdProvider : IContainerIdProvider
    {
        /// <summary>
        /// Gets null for container id. This is useful in environemnt like Windows, where there is no way to findout the contaienr id.
        /// </summary>
        /// <returns>Returns true with output container id of null.</returns>
        public bool TryGetMyContainerId(out string? containerId)
        {
            containerId = null;
            return true; 
        }
    }
}
