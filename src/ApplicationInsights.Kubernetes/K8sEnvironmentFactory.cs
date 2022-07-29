using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
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
        public async Task<IK8sEnvironment?> CreateAsync(DateTime timeoutAt, CancellationToken cancellationToken)
        {
            try
            {
                // When my pod become available and it's status become ready, we recognize the container is ready.

                // TODO: See if there's better way to fetch pod
                V1Pod myPod = await SpinWaitUntilGetPodAsync(timeoutAt, cancellationToken).ConfigureAwait(false);
                V1ContainerStatus? containerStatus = await SpinWaitContainerReadyAsync(timeoutAt, cancellationToken).ConfigureAwait(false);

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
            catch (UnauthorizedAccessException ex)
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

        private async Task<V1Pod> SpinWaitUntilGetPodAsync(DateTime timeoutAt, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            V1Pod? myPod = null;
            do
            {
                try
                {
                    myPod = await _podInfoManager.GetMyPodAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not UnauthorizedAccessException)
                {
                    _logger.LogWarning($"Query exception while trying to get pod info: {ex.Message}");
                    _logger.LogDebug(ex.ToString());
                }

                if (myPod is not null)
                {
                    stopwatch.Stop();
                    _logger.LogDebug(Invariant($"K8s pod info available in: {stopwatch.ElapsedMilliseconds} ms."));
                    return myPod;
                }

                // The time to get the container ready depends on how much time will a container to be initialized.
                // When there is readiness probe, the pod info will not be available until the initial delay of it is elapsed.
                // When there is no readiness probe, the minimum seems about 1000ms. 
                // Try invoke a probe on readiness every 500ms until the container is ready
                // Or it will timeout per the timeout settings.
                await Task.Delay(500).ConfigureAwait(false);
            } while (DateTime.Now < timeoutAt);

            _logger.LogDebug($"{nameof(SpinWaitUntilGetPodAsync)} timed out.");
            throw new InvalidOperationException("Can't find pod information in given time.");
        }

        /// <summary>
        /// Waits until the container is ready.
        /// Refer document @ https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-phase for the Pod's lifecycle.
        /// </summary>
        /// <param name="timeoutAt">A point in time when the wait on container startup is abandoned.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Returns the pod with refreshed info on it.
        /// </returns>
        private async Task<V1ContainerStatus?> SpinWaitContainerReadyAsync(DateTime timeoutAt, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                try
                {

                    if (await _containerStatusManager.IsContainerReadyAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return await _containerStatusManager.TryGetMyContainerStatusAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) when (ex is not UnauthorizedAccessException)
                {
                    _logger.LogWarning($"Query exception while trying to get container info: {ex.Message}");
                    _logger.LogDebug(ex.ToString());
                    throw;
                }

                // The time to get the container ready depends on how much time will a container to be initialized.
                // When there is readiness probe, the pod info will not be available until the initial delay of it is elapsed.
                // When there is no readiness probe, the minimum seems about 1000ms. 
                // Try invoke a probe on readiness every 500ms until the container is ready
                // Or it will timeout per the timeout settings.
                await Task.Delay(500).ConfigureAwait(false);
            } while (DateTime.Now < timeoutAt);
            throw new InvalidOperationException("Container is not ready within the given period.");
        }

        private void HandleUnauthorizedAccess(UnauthorizedAccessException exception)
        {
            _logger.LogError(
                "Unauthorized. Are you missing cluster role assignment? Refer to https://aka.ms/ai-k8s-rbac for more details. Message: {0}.",
                exception.Message);
            _logger.LogDebug(exception.ToString());
        }
    }
}
