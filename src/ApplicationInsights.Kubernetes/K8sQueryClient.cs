using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using k8s;
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
        private readonly IK8sClientService _k8sClientService;

        public K8sQueryClient(IK8sClientService k8sClientService)
        {
            _k8sClientService = k8sClientService ?? throw new ArgumentNullException(nameof(_k8sClientService));
        }

        #region Pods
        /// <summary>
        /// Gets all pods in this cluster
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<V1Pod>> GetPodsAsync(CancellationToken cancellationToken)
            => (await _k8sClientService.Client.CoreV1.ListNamespacedPodAsync(_k8sClientService.Namespace, cancellationToken: cancellationToken).ConfigureAwait(false)).Items;

        /// <summary>
        /// Gets a pod by name or null in case of no match.
        /// </summary>
        public async Task<V1Pod?> GetPodAsync(string podName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(podName))
            {
                throw new ArgumentException($"'{nameof(podName)}' cannot be null or whitespace.", nameof(podName));
            }

            V1Pod? pod = await _k8sClientService.Client.CoreV1.ReadNamespacedPodAsync(podName, _k8sClientService.Namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
            return pod;
        }
        #endregion

        #region Replica Sets
        public async Task<IEnumerable<V1ReplicaSet>> GetReplicasAsync(CancellationToken cancellationToken)
            => (await _k8sClientService.Client.AppsV1.ListNamespacedReplicaSetAsync(_k8sClientService.Namespace, cancellationToken: cancellationToken).ConfigureAwait(false)).Items;
        #endregion

        #region Deployment
        public async Task<IEnumerable<V1Deployment>> GetDeploymentsAsync(CancellationToken cancellationToken)
            => (await _k8sClientService.Client.AppsV1.ListNamespacedDeploymentAsync(_k8sClientService.Namespace, cancellationToken: cancellationToken).ConfigureAwait(false)).Items;
        #endregion

        #region Node
        public async Task<IEnumerable<V1Node>> GetNodesAsync(CancellationToken cancellationToken)
            => (await _k8sClientService.Client.CoreV1.ListNodeAsync(cancellationToken: cancellationToken)).Items;
        #endregion
    }
}
