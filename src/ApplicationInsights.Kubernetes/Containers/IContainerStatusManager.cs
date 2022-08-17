using System.Threading;
using System.Threading.Tasks;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

internal interface IContainerStatusManager
{
    Task<V1ContainerStatus?> GetMyContainerStatusAsync(CancellationToken cancellationToken);
    Task<bool> IsContainerReadyAsync(CancellationToken cancellationToken);
}
