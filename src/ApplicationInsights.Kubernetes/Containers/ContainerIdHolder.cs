using System;
using System.Collections.Generic;
using System.Linq;
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

        // If there's no container id provided providers
        IList<V1ContainerStatus>? containerStatuses = pod.Status?.ContainerStatuses;
        if (containerStatuses is not null)
        {
            // check to see if there's only 1 container inside the pod
            // else select container by environment variable.
            if (containerStatuses.Count == 1)
            {
                containerStatus = containerStatuses[0];
                _logger.LogDebug(FormattableString.Invariant($"Use the only container inside the pod for container id: {containerStatus.ContainerID}"));
            }
            else
            {
                string? containerName = Environment.GetEnvironmentVariable("ContainerName");
                _logger.LogDebug(FormattableString.Invariant($"Select container by environment variable containerName: {containerName}"));
                containerStatus = containerStatuses.FirstOrDefault(c => string.Equals(c.Name, containerName, StringComparison.Ordinal));
                if (containerStatus is not null)
                {
                    _logger.LogDebug(FormattableString.Invariant($"Selected container by container.name property container id: {containerStatus.ContainerID}"));
                }

            }
            if (containerStatus is not null)
            {
                _logger.LogInformation(FormattableString.Invariant($"Selected container id: {containerStatus.ContainerID}, name: {containerStatus.Name}"));

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
            _logger.LogError(FormattableString.Invariant($"Try back fill ContainerId failed"));
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
                    _logger.LogInformation(FormattableString.Invariant($"Got container id {containerId} by provider: {provider.GetType().Name}"));
                    return true;
                }
            }
        }

        _logger.LogInformation("No container id found by container id providers.");

        return false;
    }
}

