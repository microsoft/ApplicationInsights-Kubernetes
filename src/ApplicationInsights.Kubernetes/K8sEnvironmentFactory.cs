#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sEnvironmentFactory : IK8sEnvironmentFactory
    {
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
        private readonly IKubeHttpClientSettingsProvider _httpClientSettings;
        private readonly KubeHttpClientFactory _httpClientFactory;
        private readonly IK8sQueryClientFactory _k8sQueryClientFactory;
        private readonly IPodInfoManager _podInfoManager;

        public K8sEnvironmentFactory(
            IKubeHttpClientSettingsProvider httpClientSettingsProvider,
            KubeHttpClientFactory httpClientFactory,
            IK8sQueryClientFactory k8SQueryClientFactory,
            IPodInfoManager podInfoManager)
        {
            _httpClientSettings = Arguments.IsNotNull(httpClientSettingsProvider, nameof(httpClientSettingsProvider));
            _httpClientFactory = Arguments.IsNotNull(httpClientFactory, nameof(httpClientFactory));
            _k8sQueryClientFactory = Arguments.IsNotNull(k8SQueryClientFactory, nameof(k8SQueryClientFactory));
            _podInfoManager = podInfoManager ?? throw new ArgumentNullException(nameof(podInfoManager));
        }

        /// <summary>
        /// Async factory method to build the instance of a K8sEnvironment.
        /// </summary>
        /// <returns></returns>
        public async Task<IK8sEnvironment?> CreateAsync(DateTime timeoutAt, CancellationToken cancellationToken)
        {
            K8sEnvironment? instance = null;

            try
            {
                using (IK8sQueryClient queryClient = _k8sQueryClientFactory.Create())
                {
                    // TODO: See if there's better way to fetch the container id
                    K8sPod? myPod = await SpinWaitUntilGetPodAsync(timeoutAt, queryClient, cancellationToken).ConfigureAwait(false);
                    if (myPod == null)
                    {
                        // MyPod is null, meaning query timed out.
                        _logger.LogCritical("Fail to fetch the pod information in time. Kubernetes info will not be available for the telemetry.");
                        return null;
                    }

                    // Notice: _httpClientSettings.ContainerId is provided by various container id providers.
                    // However, there is still a chance for the container id to be empty.
                    // That is less likely to happen on Linux due to auto detection but it will happen on Windows when the variable of `ContainerId` is not properly set.
                    // We will send out a warning to let the Linux user know about code flaws. However, on both platforms, the intention is to make the container id optional.
                    string containerId = _httpClientSettings.ContainerId;

                    // Give out warnings on Linux in case the auto detect has a bug.
                    if (string.IsNullOrEmpty(containerId) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        _logger.LogWarning("Can't fetch container id. Container id info will be missing. Please file an issue at https://github.com/microsoft/ApplicationInsights-Kubernetes/issues.");
                    }

                    // Most pod has only 1 container, use it.
                    if (string.IsNullOrEmpty(containerId))
                    {
                        ContainerStatus[] containerStatuses = myPod.Status.ContainerStatuses.ToArray();
                        if (containerStatuses.Length == 1)
                        {
                            containerId = containerStatuses[0].ContainerID;
                            _logger.LogInformation(Invariant($"Use the only container inside the pod for container id: {containerId}"));
                        }
                    }

                    // Notes: It is still possible for the optional container id to be empty at this point, the following method needs to handle the case.
                    if (!await SpinWaitContainerReadyAsync(timeoutAt, queryClient, myPod, containerId, cancellationToken).ConfigureAwait(false))
                    {
                        _logger.LogError(Invariant($"Kubernetes info is not available before the timeout at {timeoutAt}."));
                        return null;
                    }

                    // Pod & container info is ready.
                    instance = new K8sEnvironment()
                    {
                        ContainerID = containerId,
                    };

                    instance.myPod = myPod;
                    _logger.LogDebug(Invariant($"Getting container status of container-id: {containerId}"));
                    instance.myContainerStatus = myPod.GetContainerStatus(containerId);

                    IEnumerable<K8sReplicaSet> replicaSetList = await queryClient.GetReplicasAsync(cancellationToken).ConfigureAwait(false);
                    instance.myReplicaSet = myPod.GetMyReplicaSet(replicaSetList);

                    if (instance.myReplicaSet is not null)
                    {
                        IEnumerable<K8sDeployment> deploymentList = await queryClient.GetDeploymentsAsync(cancellationToken).ConfigureAwait(false);
                        instance.myDeployment = instance.myReplicaSet.GetMyDeployment(deploymentList);
                    }

                    if (instance.myPod is not null)
                    {
                        IEnumerable<K8sNode> nodeList = await queryClient.GetNodesAsync(cancellationToken).ConfigureAwait(false);
                        string nodeName = instance.myPod.Spec.NodeName;
                        if (!string.IsNullOrEmpty(nodeName))
                        {
                            instance.myNode = nodeList.FirstOrDefault(node => string.Equals(node.Metadata?.Name, nodeName, StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
                return instance;
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

        private async Task<K8sPod?> SpinWaitUntilGetPodAsync(DateTime timeoutAt, IK8sQueryClient client, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            K8sPod? myPod = null;
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

            _logger.LogDebug($"{nameof(SpinWaitUntilGetPodAsync)} timed out. Return null.");
            return null;
        }

        /// <summary>
        /// Waits until the container is ready.
        /// Refer document @ https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-phase for the Pod's lifecycle.
        /// </summary>
        /// <param name="timeoutAt">A point in time when the wait on container startup is abandoned.</param>
        /// <param name="client">Query client to try getting info from the Kubernetes cluster API.</param>
        /// <param name="myPod">The pod that contains the containers.</param>
        /// <param name="myContainerId">The container that we are interested in. When string.Empty is provided, the first container inside the pod will be used to determine the container status.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<bool> SpinWaitContainerReadyAsync(DateTime timeoutAt, IK8sQueryClient client, K8sPod myPod, string myContainerId, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string podName = myPod.Metadata.Name;
            if (string.IsNullOrEmpty(podName))
            {
                throw new InvalidOperationException("Valid pod name shall never be null or empty.");
            }

            do
            {
                bool readyToGo = false;
                // It is important to fetch the pod info every iteration to get the latest status.
                K8sPod? podInfo = await _podInfoManager.GetPodByNameAsync(podName, cancellationToken).ConfigureAwait(false);
                if (podInfo is null)
                {
                    _logger.LogWarning("No pod info is fetched. This should not happen frequently.");
                    await Task.Delay(500).ConfigureAwait(false);
                    continue;
                }

                if (!string.IsNullOrEmpty(myContainerId))
                {
                    // Check targeted container status
                    readyToGo = IsContainerReady(podInfo.GetContainerStatus(myContainerId));
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
                    return true;
                }

                // The time to get the container ready depends on how much time will a container to be initialized.
                // When there is readiness probe, the pod info will not be available until the initial delay of it is elapsed.
                // When there is no readiness probe, the minimum seems about 1000ms. 
                // Try invoke a probe on readiness every 500ms until the container is ready
                // Or it will timeout per the timeout settings.
                await Task.Delay(500).ConfigureAwait(false);
            } while (DateTime.Now < timeoutAt);
            return false;
        }

        private void HandleUnauthorizedAccess(UnauthorizedAccessException exception)
        {
            _logger.LogError(
                "Unauthorized. Are you missing cluster role assignment? Refer to https://aka.ms/ai-k8s-rbac for more details. Message: {0}.",
                exception.Message);
            _logger.LogDebug(exception.ToString());
        }

        private bool IsContainerReady(ContainerStatus? containerStatus)
        {
            _logger.LogDebug("Container status object: {0}, isReady:: {1}", containerStatus, containerStatus?.Ready);
            return containerStatus is not null && containerStatus.Ready;
        }
    }
}
