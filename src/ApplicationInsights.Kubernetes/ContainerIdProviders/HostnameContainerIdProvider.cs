#nullable enable

using System;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders
{
    /// <summary>
    /// Gets current container id by environment variable of HOSTNAME.
    /// </summary>
    internal class HostnameContainerIdProvider : IContainerIdProvider
    {
        private const string _environmentVariableName = "HOSTNAME";

        /// <summary>
        /// Gets the container id by host name.
        /// </summary>
        /// <returns>
        /// Returns the value of $HOSTNAME.
        /// </returns>
        /// <remarks>
        /// The containerId retrieved from $HOSTNAME will only be the first 12 characters.
        /// Chances are, it should be good enough to be used to findout the pod for it.
        ///
        /// This should only be used as a fallback when there is no other means to get it.
        /// </remarks>
        public bool TryGetMyContainerId(out string? containerId)
        {
            containerId = Environment.GetEnvironmentVariable(_environmentVariableName);
            return containerId != null;
        }
    }
}
