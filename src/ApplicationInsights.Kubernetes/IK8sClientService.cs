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
    Task<IEnumerable<V1Pod>> GetPodsAsync(CancellationToken cancellationToken);

    Task<V1Pod?> GetPodByNameAsync(string podName, CancellationToken cancellationToken);

    Task<IEnumerable<V1ReplicaSet>> GetReplicaSetsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<V1Deployment>> GetDeploymentsAsync(CancellationToken cancellationToken);

    Task<IEnumerable<V1Node>> GetNodesAsync(bool ignoreForbiddenException, CancellationToken cancellationToken);
}
