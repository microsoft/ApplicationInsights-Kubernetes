using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Entities;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IK8sQueryClient
    {
        Task<IEnumerable<K8sDeployment>> GetDeploymentsAsync();
        Task<IEnumerable<K8sNode>> GetNodesAsync();
        Task<IEnumerable<K8sPod>> GetPodsAsync();
        Task<IEnumerable<K8sReplicaSet>> GetReplicasAsync();
    }
}
