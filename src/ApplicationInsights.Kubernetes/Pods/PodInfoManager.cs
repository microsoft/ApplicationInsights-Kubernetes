using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

internal class PodInfoManager : IPodInfoManager
{
    private readonly IK8sClientService _k8SClient;
    private readonly IContainerIdHolder _containerIdHolder;
    private readonly IEnumerable<IPodNameProvider> _podNameProviders;
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    public PodInfoManager(
        IK8sClientService k8sClient,
        IContainerIdHolder containerIdHolder,
        IEnumerable<IPodNameProvider> podNameProviders)
    {
        _k8SClient = k8sClient ?? throw new ArgumentNullException(nameof(k8sClient));
        _containerIdHolder = containerIdHolder ?? throw new ArgumentNullException(nameof(containerIdHolder));
        _podNameProviders = podNameProviders ?? throw new System.ArgumentNullException(nameof(podNameProviders));
    }

    /// <inheritdoc />
    public async Task<V1Pod?> GetMyPodAsync(CancellationToken cancellationToken)
    {
        string podName = string.Empty;
        foreach (IPodNameProvider podNameProvider in _podNameProviders)
        {
            _logger.LogDebug($"Getting pod name by provider: {podNameProvider.GetType().Name}");
            if (podNameProvider.TryGetPodName(out podName))
            {
                // Got pod name successfully.
                break;
            }
        }
        _logger.LogDebug($"Pod name by providers: {podName}");

        // Find pod by name
        if (!string.IsNullOrEmpty(podName))
        {
            V1Pod? targetPod = await GetPodByNameAsync(podName, cancellationToken).ConfigureAwait(false);
            if (targetPod is not null)
            {
                _logger.LogInformation($"Found pod by name providers: {targetPod.Metadata?.Name}");
                return targetPod;
            }
        }
        else
        {
            _logger.LogTrace("Pod name is not fetched by providers.");
        }

        // Find pod by container id:
        string? myContainerId = _containerIdHolder.ContainerId;
        _logger.LogDebug($"Looking for pod name by container id: {myContainerId}");

        if (!string.IsNullOrEmpty(myContainerId))
        {
            V1Pod? targetPod = null;
            IEnumerable<V1Pod> allPods = await _k8SClient.GetPodsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            targetPod = allPods.FirstOrDefault(pod => TryGetContainerStatus(pod, myContainerId, out _));

            if (targetPod is not null)
            {
                _logger.LogInformation($"Found pod by container id. Pod name: {targetPod.Metadata?.Name}");
                return targetPod;
            }
        }
        else
        {
            _logger.LogTrace("Container id is not available for use to locate pod.");
        }

        _logger.LogError("Pod name can't be determined.");
        return null;
    }

    /// <inheritdoc />
    public Task<V1Pod?> GetPodByNameAsync(string podName, CancellationToken cancellationToken)
        => _k8SClient.GetPodByNameAsync(podName, cancellationToken);

    public bool TryGetContainerStatus(V1Pod pod, string? containerId, out V1ContainerStatus? containerStatus)
    {
        containerStatus = string.IsNullOrEmpty(containerId) ? null // Special case when container id is an empty string,
                    : pod.Status?.ContainerStatuses?.FirstOrDefault(
                        // Notice, we are using partial matching because there could be prefix of container ids like: docker://b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4
                        status =>
                            (!string.IsNullOrEmpty(status.ContainerID) && status.ContainerID.IndexOf(containerId, StringComparison.OrdinalIgnoreCase) != -1) ||
                            (!string.IsNullOrEmpty(status.ImageID) && status.ImageID.IndexOf(containerId, StringComparison.OrdinalIgnoreCase) != -1)    // In some environment, the id is ImageID rather than ContainerId
                        );

        return containerStatus is not null;
    }
}
