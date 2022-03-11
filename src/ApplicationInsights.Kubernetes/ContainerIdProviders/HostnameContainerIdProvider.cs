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
        /// A host name is only first 12 characters of the container name in Linux.
        /// This should only be used as a fallback when there is no other means to get it.
        /// </remarks>
        public bool TryGetMyContainerId(out string? containerId)
        {
            containerId = Environment.GetEnvironmentVariable(_environmentVariableName);
            return containerId != null;
        }
    }
}
