using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
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
        private readonly IK8sClientService _k8sClient;

        public K8sEnvironmentFactory(
            IContainerIdHolder containerIdHolder,
            IPodInfoManager podInfoManager,
            IK8sClientService k8sClient)
        {
            _containerIdHolder = containerIdHolder ?? throw new ArgumentNullException(nameof(containerIdHolder));
            _podInfoManager = podInfoManager ?? throw new ArgumentNullException(nameof(podInfoManager));
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
                // TODO: See if there's better way to fetch the container id
                V1Pod myPod = await SpinWaitUntilGetPodAsync(timeoutAt, cancellationToken).ConfigureAwait(false);

                // Notice: _httpClientSettings.ContainerId is provided by various container id providers.
                // However, there is still a chance for the container id to be empty.
                // That is less likely to happen on Linux due to auto detection but it will happen on Windows when the variable of `ContainerId` is not properly set.
                // We will send out a warning to let the Linux user know about code flaws. However, on both platforms, the intention is to make the container id optional.
                string? containerId = _containerIdHolder.ContainerId;

                // Give out warnings on Linux in case the auto detect has a bug.
                if (string.IsNullOrEmpty(containerId) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _logger.LogWarning("Can't fetch container id. Container id info will be missing. Please file an issue at https://github.com/microsoft/ApplicationInsights-Kubernetes/issues.");
                }

                // Most pod has only 1 container, use it.
                if (string.IsNullOrEmpty(containerId))
                {
                    V1ContainerStatus[] containerStatuses = myPod.Status.ContainerStatuses.ToArray();
                    if (containerStatuses.Length == 1)
                    {
                        containerId = containerStatuses[0].ContainerID;
                        _logger.LogInformation(Invariant($"Use the only container inside the pod for container id: {containerId}"));
                    }
                }

                // Notes: It is still possible for the optional container id to be empty at this point, the following method needs to handle the case.
                V1ContainerStatus? containerStatus = null;
                (myPod, containerStatus) = await SpinWaitContainerReadyAsync(timeoutAt, myPod.Metadata.Name, containerId, cancellationToken).ConfigureAwait(false);

                // Pod & container info is ready.
                // Try refresh pod info unless it doesn't exist.
                myPod = (await _podInfoManager.GetMyPodAsync(cancellationToken).ConfigureAwait(false)) ?? myPod;

                // Fetch container status unless container id is null.
                if (!string.IsNullOrEmpty(containerId))
                {
                    containerStatus = myPod.GetContainerStatus(containerId);
                }
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
                // When my pod become available and it's status become ready, we recognize the container is ready.
                try
                {
                    myPod = await _podInfoManager.GetMyPodAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException ex)
                {
                    HandleUnauthorizedAccess(ex);
                    myPod = null;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
                {
                    _logger.LogWarning($"Query exception while trying to get pod info: {ex.Message}");
                    _logger.LogDebug(ex.ToString());
                    myPod = null;
                }
#pragma warning restore CA1031 // Do not catch general exception types

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
        /// <param name="podName">Name of the pod that contains the containers.</param>
        /// <param name="myContainerId">The container that we are interested in. When string.Empty is provided, the first container inside the pod will be used to determine the container status.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Returns the pod with refreshed info on it.
        /// </returns>
        private async Task<(V1Pod, V1ContainerStatus?)> SpinWaitContainerReadyAsync(DateTime timeoutAt, string podName, string? myContainerId, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (string.IsNullOrEmpty(podName))
            {
                throw new InvalidOperationException("Valid pod name shall never be null or empty.");
            }

            do
            {
                bool readyToGo = false;
                // It is important to fetch the pod info every iteration to get the latest status.
                V1Pod? podInfo = await _podInfoManager.GetPodByNameAsync(podName, cancellationToken).ConfigureAwait(false);
                if (podInfo is null)
                {
                    _logger.LogWarning("No pod info is fetched. This should not happen frequently.");
                    await Task.Delay(500).ConfigureAwait(false);
                    continue;
                }

                V1ContainerStatus? containerStatus = null;
                if (!string.IsNullOrEmpty(myContainerId))
                {
                    // Check targeted container status
                    containerStatus = podInfo.GetContainerStatus(myContainerId);
                    readyToGo = IsContainerReady(containerStatus);
                }
                else
                {
                    _logger.LogWarning("No container id available. Fallback to use the any container for status checking.");
                    readyToGo = podInfo.GetAllContainerStatus().Any(s => IsContainerReady(s));
                }

                if (readyToGo)
                {
                    stopwatch.Stop();
                    _logger.LogDebug(Invariant($"K8s container info available in: {stopwatch.ElapsedMilliseconds} ms."));
                    return (podInfo, containerStatus);
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

        private bool IsContainerReady(V1ContainerStatus? containerStatus)
        {
            _logger.LogTrace($"Container status object: {containerStatus}, isReady: {containerStatus?.Ready}");
            return containerStatus is not null && containerStatus.Ready;
        }
    }
}
