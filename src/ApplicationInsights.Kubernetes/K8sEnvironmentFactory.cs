using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Microsoft.Rest;
using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sEnvironmentFactory : IK8sEnvironmentFactory
    {
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
        private readonly IContainerIdHolder _containerIdHolder;
        private readonly IPodInfoManager _podInfoManager;
        private readonly IContainerStatusManager _containerStatusManager;
        private readonly IK8sClientService _k8sClient;

        public K8sEnvironmentFactory(
            IContainerIdHolder containerIdHolder,
            IPodInfoManager podInfoManager,
            IContainerStatusManager containerStatusManager,
            IK8sClientService k8sClient)
        {
            _containerIdHolder = containerIdHolder ?? throw new ArgumentNullException(nameof(containerIdHolder));
            _podInfoManager = podInfoManager ?? throw new ArgumentNullException(nameof(podInfoManager));
            _containerStatusManager = containerStatusManager ?? throw new ArgumentNullException(nameof(containerStatusManager));
            _k8sClient = k8sClient ?? throw new ArgumentNullException(nameof(k8sClient));
        }

        /// <summary>
        /// Async factory method to build the instance of a K8sEnvironment.
        /// </summary>
        /// <returns></returns>
        public async Task<IK8sEnvironment?> CreateAsync(CancellationToken cancellationToken)
        {
            try
            {
                // When my pod become available and it's status become ready, we recognize the container is ready.

                // TODO: See if there's better way to fetch pod
                V1Pod myPod = await SpinWaitUntilGetPodAsync(cancellationToken).ConfigureAwait(false);
                V1ContainerStatus? containerStatus = await SpinWaitContainerReadyAsync(cancellationToken).ConfigureAwait(false);

                // Fetch replica set info
                IEnumerable<V1ReplicaSet> allReplicaSet = await _k8sClient.GetReplicaSetsAsync(cancellationToken).ConfigureAwait(false);
                V1ReplicaSet? replicaSet = myPod.GetMyReplicaSet(allReplicaSet);

                // Fetch deployment info
                IEnumerable<V1Deployment> allDeployment = await _k8sClient.GetDeploymentsAsync(cancellationToken).ConfigureAwait(false);
                V1Deployment? deployment = replicaSet?.GetMyDeployment(allDeployment);

                // Fetch node info
                string nodeName = myPod.Spec.NodeName;
                IEnumerable<V1Node> allNodes = await _k8sClient.GetNodesAsync(cancellationToken).ConfigureAwait(false);
                V1Node? node = allNodes.FirstOrDefault(n => string.Equals(n.Metadata.Name, nodeName, StringComparison.Ordinal));

                return new K8sEnvironment(containerStatus, myPod, replicaSet, deployment, node);
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Forbidden)
            {
                HandleUnauthorizedAccess(ex);
                return null;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                return null;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private async Task<V1Pod> SpinWaitUntilGetPodAsync(CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            V1Pod? myPod = null;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    myPod = await _podInfoManager.GetMyPodAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not HttpOperationException || (ex is HttpOperationException operationException && operationException.Response.StatusCode != HttpStatusCode.Forbidden))
                {
                    _logger.LogWarning($"Query exception while trying to get pod info: {ex.Message}");
                    _logger.LogDebug(ex.ToString());
                }

                stopwatch.Stop();
                if (myPod is not null)
                {
                    _logger.LogDebug(Invariant($"K8s pod info available in: {stopwatch.ElapsedMilliseconds} ms."));
                    return myPod;
                }

                // The time to get the container ready depends on how much time will a container to be initialized.
                // When there is readiness probe, the pod info will not be available until the initial delay of it is elapsed.
                // When there is no readiness probe, the minimum seems about 1000ms. 
                // Try invoke a probe on readiness every 500ms until the container is ready
                // Or it will timeout per the timeout settings.
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: cancellationToken).ConfigureAwait(false);
            } while (true);
        }

        /// <summary>
        /// Waits until the container is ready.
        /// Refer document @ https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-phase for the Pod's lifecycle.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Returns the pod with refreshed info on it.
        /// </returns>
        private async Task<V1ContainerStatus?> SpinWaitContainerReadyAsync(CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (await _containerStatusManager.IsContainerReadyAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return await _containerStatusManager.GetMyContainerStatusAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) when (ex is not HttpOperationException || (ex is HttpOperationException operationException && operationException.Response.StatusCode != HttpStatusCode.Forbidden))
                {
                    _logger.LogWarning($"Query exception while trying to get container info: {ex.Message}");
                    _logger.LogDebug(ex.ToString());
                }

                // The time to get the container ready depends on how much time will a container to be initialized.
                // When there is readiness probe, the pod info will not be available until the initial delay of it is elapsed.
                // When there is no readiness probe, the minimum seems about 1000ms. 
                // Try invoke a probe on readiness every 500ms until the container is ready
                // Or it will timeout per the timeout settings.
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: cancellationToken).ConfigureAwait(false);
            } while (true);
        }

        private void HandleUnauthorizedAccess(HttpOperationException exception)
        {
            Debug.Assert(exception.Response.StatusCode == HttpStatusCode.Forbidden, "Only handle Forbidden!");
            _logger.LogError(
                "Unauthorized. Are you missing cluster role assignment? Refer to https://aka.ms/ai-k8s-rbac for more details. Message: {0}.",
                exception.Message);
            _logger.LogDebug(exception.ToString());
        }
    }
}
