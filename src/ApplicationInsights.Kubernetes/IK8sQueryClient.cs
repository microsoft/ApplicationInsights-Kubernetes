#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Entities;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IK8sQueryClient
    {
        Task<IEnumerable<K8sDeployment>> GetDeploymentsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<K8sNode>> GetNodesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<K8sPod>> GetPodsAsync(CancellationToken cancellationToken);
        Task<K8sPod?> GetPodAsync(string podName, CancellationToken cancellationToken);
        Task<IEnumerable<K8sReplicaSet>> GetReplicasAsync(CancellationToken cancellationToken);
    }
}
