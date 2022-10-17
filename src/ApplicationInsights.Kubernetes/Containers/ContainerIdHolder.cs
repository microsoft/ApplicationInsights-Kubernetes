using System;
using System.Collections.Generic;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

/// <summary>
/// A cache to hold the container id.
/// </summary>
internal class ContainerIdHolder : IContainerIdHolder
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private string? _containerId;

    public ContainerIdHolder(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public string? ContainerId
    {
        get
        {
            if (string.IsNullOrEmpty(_containerId))
            {
                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    IContainerIdNormalizer normalizer = scope.ServiceProvider.GetRequiredService<IContainerIdNormalizer>();
                    _ = TryGetContainerId(out _containerId) && normalizer.TryNormalize(_containerId!, out _containerId);
                }
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

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IContainerIdNormalizer normalizer = scope.ServiceProvider.GetRequiredService<IContainerIdNormalizer>();
                if (normalizer.TryNormalize(containerStatus.ContainerID, out string? normalizedContainerId))
                {
                    _containerId = normalizedContainerId;
                    return true;
                }
            }

            _logger.LogError(FormattableString.Invariant($"Normalization failed for container id: {containerStatus.ContainerID}"));
        }
        return false;
    }

    private bool TryGetContainerId(out string? containerId)
    {
        containerId = string.Empty;
        using (IServiceScope scope = _serviceScopeFactory.CreateScope())
        {
            foreach (IContainerIdProvider provider in scope.ServiceProvider.GetServices<IContainerIdProvider>())
            {
                if (provider.TryGetMyContainerId(out containerId))
                {
                    _logger.LogInformation(FormattableString.Invariant($"Get container id by provider: {containerId}"));
                    return true;
                }
            }
        }

        _logger.LogInformation("No container id found by container id providers.");

        return false;
    }
}

