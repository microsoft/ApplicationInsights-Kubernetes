using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

internal class ContainerStatusManager : IContainerStatusManager
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    private readonly IPodInfoManager _podInfoManager;
    private readonly IContainerIdHolder _containerIdHolder;

    public ContainerStatusManager(
        IPodInfoManager podInfoManager,
        IContainerIdHolder containerIdHolder)
    {
        _podInfoManager = podInfoManager ?? throw new System.ArgumentNullException(nameof(podInfoManager));
        _containerIdHolder = containerIdHolder ?? throw new System.ArgumentNullException(nameof(containerIdHolder));
    }

    public async Task<bool> IsContainerReadyAsync(CancellationToken cancellationToken)
    {
        V1ContainerStatus? myContainerStatus = await TryGetMyContainerStatusAsync(cancellationToken).ConfigureAwait(false);
        if (myContainerStatus is not null)
        {
            return IsContainerStatusReady(myContainerStatus);
        }

        return false;
    }

    public async Task<V1ContainerStatus?> TryGetMyContainerStatusAsync(CancellationToken cancellationToken)
    {
        // Always get the latest status by querying the pod object
        V1Pod? myPod = await _podInfoManager.GetMyPodAsync(cancellationToken).ConfigureAwait(false);
        if (myPod is null)
        {
            return null;
        }

        string? containerId = _containerIdHolder.ContainerId;

        //Known container id
        if (!string.IsNullOrEmpty(containerId))
        {
            if (_podInfoManager.TryGetContainerStatus(myPod, containerId, out V1ContainerStatus? foundContainerStatus))
            {
                if (!string.Equals(foundContainerStatus?.ContainerID, containerId, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Container ID fetched doesn't match the container id on container status. Probably matched with the image id.");
                    _containerIdHolder.TryBackFillContainerId(foundContainerStatus!);
                }

                return foundContainerStatus;
            }
        }

        // If there's no container id provided by the container id holder, at this moment, try backfill
        // Give out warnings on Linux in case the auto detect has a bug.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _logger.LogWarning("Can't fetch container id. Container id info will be missing. Please file an issue at https://github.com/microsoft/ApplicationInsights-Kubernetes/issues.");
        }

        if (_containerIdHolder.TryBackFillContainerId(myPod, out V1ContainerStatus? inferredContainerStatus))
        {
            // Back fill success, return the status.
            return inferredContainerStatus;
        }

        // There's no container id by holder, and it is not single container pod, can't determine the status.
        return null;
    }

    private bool IsContainerStatusReady(V1ContainerStatus containerStatus)
    {
        _logger.LogTrace($"Container status object: {containerStatus}, isReady: {containerStatus?.Ready}");
        return containerStatus is not null && containerStatus.Ready;
    }
}

