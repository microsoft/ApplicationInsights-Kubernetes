using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using k8s.Autorest;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Pods;

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
        V1ContainerStatus? myContainerStatus = await GetMyContainerStatusAsync(cancellationToken).ConfigureAwait(false);

        // Found my container status, return if it is ready
        if (myContainerStatus is not null)
        {
            return IsContainerStatusReady(myContainerStatus);
        }

        // Can not locate my container, fall back to any container's status
        await foreach (V1ContainerStatus status in GetAnyContainerStatusAsync(cancellationToken).ConfigureAwait(false))
        {
            if (status.Ready)
            {
                return true;
            }
        }

        // No container is ready.
        return false;
    }

    public async Task<V1ContainerStatus?> GetMyContainerStatusAsync(CancellationToken cancellationToken)
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
            // There is a container id, check the status
            if (_podInfoManager.TryGetContainerStatus(myPod, containerId, out V1ContainerStatus? foundContainerStatus))
            {
                // Found status
                return foundContainerStatus;
            }
            // Container status not ready yet.
            return null;
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

    private async IAsyncEnumerable<V1ContainerStatus> GetAnyContainerStatusAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Always get the latest status by querying the pod object
        V1Pod? myPod = await _podInfoManager.GetMyPodAsync(cancellationToken).ConfigureAwait(false);
        if (myPod?.Status?.ContainerStatuses is null)
        {
            yield break;
        }

        foreach (V1ContainerStatus containerStatus in myPod.Status.ContainerStatuses)
        {
            yield return containerStatus;
        }
    }

    /// <summary>
    /// Get container readiness.
    /// </summary>
    /// <param name="containerStatus">The container status if any.</param>
    /// <returns></returns>
    private bool IsContainerStatusReady(V1ContainerStatus containerStatus)
    {
        _logger.LogTrace($"Container status object: {containerStatus}, isReady: {containerStatus?.Ready}");
        return containerStatus is not null && containerStatus.Ready;
    }

    /// <summary>
    /// Wait until the container is ready. In case container id is absent, it waits for any container in the pod got ready.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>My container status or null.</returns>
    public async Task<V1ContainerStatus?> WaitContainerReadyAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (await IsContainerReadyAsync(cancellationToken).ConfigureAwait(false))
                {
                    // Notes: it is still possible to my container status to be null, because IsContainerReadyAsync above checks for
                    // either my container or any container's status when my container id is absent.
                    return await GetMyContainerStatusAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is not HttpOperationException || (ex is HttpOperationException operationException && operationException.Response.StatusCode != HttpStatusCode.Forbidden))
            {
                _logger.LogWarning($"Query exception while trying to get container info: {ex.Message}");
                _logger.LogDebug(ex.ToString());
            }

            // The time to get the container ready depends on how much time will a container to be initialized.
            // When there is readiness probe, the pod info will not be available until the initial delay of it is elapsed.
            // When there is no readiness probe, the minimum seems about 1000ms. 
            // Try invoke a probe on readiness every 500ms until the container is ready
            // Or it will timeout per the timeout settings.
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}

