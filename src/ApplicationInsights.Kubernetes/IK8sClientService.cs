using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A simple wrapper for IKubernetes to provide encapsulation and testability to 
/// IKubernetes' extension methods.
/// </summary>
internal interface IK8sClientService
{
    Task<IEnumerable<V1Pod>> ListPodsAsync(CancellationToken cancellationToken);

    Task<V1Pod?> GetPodByNameAsync(string podName, CancellationToken cancellationToken);

    Task<IEnumerable<V1ReplicaSet>> ListReplicaSetsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<V1Deployment>> ListDeploymentsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<V1Node>> ListNodesAsync(CancellationToken cancellationToken);
}
