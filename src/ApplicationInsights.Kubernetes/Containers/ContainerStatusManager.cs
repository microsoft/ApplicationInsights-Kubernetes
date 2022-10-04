using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Pods;
using Microsoft.Rest;

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
        if (myContainerStatus is not null)
        {
            return IsContainerStatusReady(myContainerStatus);
        }

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

    private bool IsContainerStatusReady(V1ContainerStatus containerStatus)
    {
        _logger.LogTrace($"Container status object: {containerStatus}, isReady: {containerStatus?.Ready}");
        return containerStatus is not null && containerStatus.Ready;
    }

    public async Task<V1ContainerStatus?> WaitContainerReadyAsync(CancellationToken cancellationToken)
    {
        while(true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (await IsContainerReadyAsync(cancellationToken).ConfigureAwait(false))
                {
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

