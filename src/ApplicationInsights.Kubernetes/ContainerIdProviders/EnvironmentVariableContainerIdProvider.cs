#nullable enable

using System;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders
{
    internal class EnvironmentVariableContainerIdProvider : IContainerIdProvider
    {
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        private const string EnvironmentVariableName = "ContainerId";

        /// <summary>
        /// Try get the container id by environment variable named "ContainerId".
        /// </summary>
        public bool TryGetMyContainerId(out string? containerId)
        {
            containerId = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            _logger.LogWarning($"Getting container id by environment variable {EnvironmentVariableName}. Result: {containerId}.");
            return !string.IsNullOrEmpty(containerId);
        }
    }
}
