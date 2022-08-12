using System;
using System.Collections.Generic;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

/// <summary>
/// A cache to hold the container id.
/// </summary>
internal class ContainerIdHolder : IContainerIdHolder
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    private readonly IEnumerable<IContainerIdProvider> _containerIdProviders;
    private readonly IContainerIdNormalizer _containerIdNormalizer;
    private string? _containerId;

    public ContainerIdHolder(
        IEnumerable<IContainerIdProvider> containerIdProviders,
        IContainerIdNormalizer containerIdNormalizer)
    {
        _containerIdProviders = containerIdProviders ?? throw new System.ArgumentNullException(nameof(containerIdProviders));
        _containerIdNormalizer = containerIdNormalizer ?? throw new ArgumentNullException(nameof(containerIdNormalizer));
    }

    public string? ContainerId
    {
        get
        {
            if (string.IsNullOrEmpty(_containerId))
            {
                _ = TryGetContainerId(out _containerId) && _containerIdNormalizer.TryNormalize(_containerId!, out _containerId);
            }
            return _containerId;
        }
    }

    public bool TryBackFillContainerId(V1Pod pod, out V1ContainerStatus? containerStatus)
    {
        if (pod is null)
        {
            throw new ArgumentNullException(nameof(pod));
        }
        containerStatus = null;

        // If there's no container id provided providers, check to see if there's only 1 container inside the pod
        IList<V1ContainerStatus>? containerStatuses = pod.Status?.ContainerStatuses;
        if (containerStatuses is not null && containerStatuses.Count == 1)
        {
            containerStatus = containerStatuses[0];
            _logger.LogInformation(FormattableString.Invariant($"Use the only container inside the pod for container id: {containerStatus.ContainerID}"));

            if (_containerIdNormalizer.TryNormalize(containerStatus.ContainerID, out string? normalizedContainerId))
            {
                _containerId = normalizedContainerId;
                return true;
            }

            _logger.LogError(FormattableString.Invariant($"Normalization failed for container id: {containerStatus.ContainerID}"));
        }
        return false;
    }

    private bool TryGetContainerId(out string? containerId)
    {
        containerId = null;
        foreach (IContainerIdProvider provider in _containerIdProviders)
        {
            if (provider.TryGetMyContainerId(out containerId))
            {
                _logger.LogInformation(FormattableString.Invariant($"Get container id by provider: {containerId}"));
                return true;
            }
        }
        return false;
    }
}

