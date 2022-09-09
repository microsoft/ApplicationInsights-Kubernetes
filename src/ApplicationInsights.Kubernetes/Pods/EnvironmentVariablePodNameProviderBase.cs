using System;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.Pods;

/// <summary>
/// Gets current container id by given environment variable.
/// </summary>
internal abstract class EnvironmentVariablePodNameProviderBase : IPodNameProvider
{
    private readonly string _environmentVariableName;
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    public EnvironmentVariablePodNameProviderBase(string environmentVariableName)
    {
        if (string.IsNullOrEmpty(environmentVariableName))
        {
            throw new ArgumentException($"'{nameof(environmentVariableName)}' cannot be null or empty.", nameof(environmentVariableName));
        }

        _environmentVariableName = environmentVariableName;
    }


    public bool TryGetPodName(out string podName)
    {
        podName = Environment.GetEnvironmentVariable(_environmentVariableName) ?? string.Empty;
        _logger.LogDebug($"Try getting pod name by environment variable {_environmentVariableName}. Result: {podName}");
        return !string.IsNullOrEmpty(podName);
    }
}
