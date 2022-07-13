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
}
