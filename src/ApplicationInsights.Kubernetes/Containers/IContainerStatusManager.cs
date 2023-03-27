using System.Threading;
using System.Threading.Tasks;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

internal interface IContainerStatusManager
{
    /// <summary>
    /// Gets my container status when available, or null.
    /// </summary>
    Task<V1ContainerStatus?> GetMyContainerStatusAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets whether my container is ready when the container id is available or check if any of the container's ready 
    /// in case container id is missing.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<bool> IsContainerReadyAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Waits until the container is ready.
    /// Refer document @ https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-phase for the Pod's lifecycle.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Returns the container status.
    /// </returns>
    Task<V1ContainerStatus?> WaitContainerReadyAsync(CancellationToken cancellationToken);
}
