using System;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

internal interface IContainerStatusManager
{
    Task<V1ContainerStatus?> GetMyContainerStatusAsync(CancellationToken cancellationToken);

    [Obsolete("Stop waiting for container ready. Use GetMyContainerStatusAsync instead.", error: true)]
    Task<bool> IsContainerReadyAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Waits until the container is ready.
    /// Refer document @ https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-phase for the Pod's lifecycle.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Returns the container status.
    /// </returns>
    [Obsolete("Stop waiting for container ready. Use GetMyContainerStatusAsync instead.", error: true)]
    Task<V1ContainerStatus?> WaitContainerReadyAsync(CancellationToken cancellationToken);
}
