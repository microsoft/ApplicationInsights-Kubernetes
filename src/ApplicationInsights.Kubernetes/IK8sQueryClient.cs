using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IK8sQueryClient
    {
        Task<IEnumerable<V1Deployment>> GetDeploymentsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<V1Node>> GetNodesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<V1Pod>> GetPodsAsync(CancellationToken cancellationToken);
        Task<V1Pod?> GetPodAsync(string podName, CancellationToken cancellationToken);
        Task<IEnumerable<V1ReplicaSet>> GetReplicasAsync(CancellationToken cancellationToken);
    }
}
