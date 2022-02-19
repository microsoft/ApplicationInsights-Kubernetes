#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders
{
    /// <summary>
    /// A service that tries to get container id.
    /// </summary>
    internal interface IContainerIdProvider
    {
        /// <summary>
        /// Trys to get the container id.
        /// </summary>
        /// <returns>True with value when attempt successfully. False with null when failed.</returns>
        bool TryGetMyContainerId(out string? containerId);
    }
}
