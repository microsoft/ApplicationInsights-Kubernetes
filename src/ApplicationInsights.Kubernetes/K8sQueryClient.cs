using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes
{

    /// <summary>
    /// High level query client for K8s concepts.
    /// </summary>
    internal class K8sQueryClient : IK8sQueryClient
    {
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
        internal IK8sClientService K8sClientService { get; }

        public K8sQueryClient(IK8sClientService k8sClientService)
        {
            K8sClientService = k8sClientService ?? throw new ArgumentNullException(nameof(k8sClientService));
        }

        #region Pods
        /// <summary>
        /// Gets all pods in this cluster
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<V1Pod>> GetPodsAsync(CancellationToken cancellationToken)
            => K8sClientService.ListPodsAsync(cancellationToken: cancellationToken);

        /// <summary>
        /// Gets a pod by name or null in case of no match.
        /// </summary>
        public async Task<V1Pod?> GetPodAsync(string podName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(podName))
            {
                throw new ArgumentException($"'{nameof(podName)}' cannot be null or whitespace.", nameof(podName));
            }

            V1Pod? pod = await K8sClientService.GetPodByNameAsync(podName, cancellationToken: cancellationToken).ConfigureAwait(false);
            return pod;
        }
        #endregion

        #region Replica Sets
        public Task<IEnumerable<V1ReplicaSet>> GetReplicasAsync(CancellationToken cancellationToken)
            => K8sClientService.ListReplicaSetsAsync(cancellationToken: cancellationToken);
        #endregion

        #region Deployment
        public Task<IEnumerable<V1Deployment>> GetDeploymentsAsync(CancellationToken cancellationToken)
            => K8sClientService.ListDeploymentsAsync(cancellationToken: cancellationToken);
        #endregion

        #region Node
        public Task<IEnumerable<V1Node>> GetNodesAsync(CancellationToken cancellationToken)
            => K8sClientService.ListNodesAsync(cancellationToken: cancellationToken);
        #endregion
    }
}
