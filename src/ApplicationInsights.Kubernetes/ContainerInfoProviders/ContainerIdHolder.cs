using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A cache to hold the container id.
/// </summary>
internal class ContainerIdHolder : IContainerIdHolder
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    private readonly IEnumerable<IContainerIdProvider> _containerIdProviders;
    private string? _containerId;

    public ContainerIdHolder(IEnumerable<IContainerIdProvider> containerIdProviders)
    {
        _containerIdProviders = containerIdProviders ?? throw new System.ArgumentNullException(nameof(containerIdProviders));
    }

    public string? ContainerId
    {
        get
        {
            if (string.IsNullOrEmpty(_containerId))
            {
                _containerId = TryGetContainerId();
            }
            return _containerId;
        }
    }

    private string? TryGetContainerId()
    {
        foreach (IContainerIdProvider provider in _containerIdProviders)
        {
            if (provider.TryGetMyContainerId(out string? containerId))
            {
                _logger.LogInformation(FormattableString.Invariant($"Get container id by provider: {containerId}"));
                return containerId;
            }
        }
        return null;
    }
}

