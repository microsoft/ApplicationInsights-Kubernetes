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
            _logger.LogDebug($"Getting container id by environment variable {EnvironmentVariableName}. Result: {containerId}.");

            bool result = !string.IsNullOrEmpty(containerId);
            if (result)
            {
                _logger.LogInformation($"[{nameof(EnvironmentVariableContainerIdProvider)}] Found container id: {containerId}");
            }
            return result;
        }
    }
}
