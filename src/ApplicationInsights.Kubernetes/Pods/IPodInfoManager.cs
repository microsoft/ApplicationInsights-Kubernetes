using System.Threading;
using System.Threading.Tasks;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

internal interface IPodInfoManager
{
    /// <summary>
    /// Tries to get the pod.
    /// </summary>
    /// <returns>Returns the K8s Pod entity when located. Otherwise, null.</returns>
    Task<V1Pod?> GetMyPodAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a pod by its name or null.
    /// </summary>
    /// <param name="podName">The target pod name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<V1Pod?> GetPodByNameAsync(string podName, CancellationToken cancellationToken);

    /// <summary>
    /// Tries to get container status from a pod by given container id.
    /// </summary>
    bool TryGetContainerStatus(V1Pod pod, string? containerId, out V1ContainerStatus? containerStatus);
}