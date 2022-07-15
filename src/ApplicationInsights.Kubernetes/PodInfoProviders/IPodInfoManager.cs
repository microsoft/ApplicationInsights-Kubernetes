#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Entities;

namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

internal interface IPodInfoManager
{
    /// <summary>
    /// Tries to get the pod.
    /// </summary>
    /// <returns>Returns the K8s Pod entity when located. Otherwise, null.</returns>
    Task<K8sPod?> GetMyPodAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a pod by its name or null.
    /// </summary>
    /// <param name="podName">The target pod name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<K8sPod?> GetPodByNameAsync(string podName, CancellationToken cancellationToken);
}
