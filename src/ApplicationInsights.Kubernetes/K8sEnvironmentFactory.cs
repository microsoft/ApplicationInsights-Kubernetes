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
        private readonly K8sQueryClientFactory _k8sQueryClientFactory;
        private readonly IPodInfoManager _podInfoManager;

        public K8sEnvironmentFactory(
            IKubeHttpClientSettingsProvider httpClientSettingsProvider,
            KubeHttpClientFactory httpClientFactory,
            K8sQueryClientFactory k8SQueryClientFactory,
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
                using (IKubeHttpClient httpClient = _httpClientFactory.Create(_httpClientSettings))
                using (K8sQueryClient queryClient = _k8sQueryClientFactory.Create(httpClient))
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

                    IEnumerable<K8sReplicaSet> replicaSetList = await queryClient.GetReplicasAsync().ConfigureAwait(false);
                    instance.myReplicaSet = myPod.GetMyReplicaSet(replicaSetList);

                    if (instance.myReplicaSet is not null)
                    {
                        IEnumerable<K8sDeployment> deploymentList = await queryClient.GetDeploymentsAsync().ConfigureAwait(false);
                        instance.myDeployment = instance.myReplicaSet.GetMyDeployment(deploymentList);
                    }

                    if (instance.myPod is not null)
                    {
                        IEnumerable<K8sNode> nodeList = await queryClient.GetNodesAsync().ConfigureAwait(false);
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

        private async Task<K8sPod?> SpinWaitUntilGetPodAsync(DateTime timeoutAt, K8sQueryClient client, CancellationToken cancellationToken)
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
                    _logger.LogDebug(Invariant($"K8s info available in: {stopwatch.ElapsedMilliseconds} ms."));
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
        private async Task<bool> SpinWaitContainerReadyAsync(DateTime timeoutAt, K8sQueryClient client, K8sPod myPod, string myContainerId, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                if (string.IsNullOrEmpty(myContainerId))
                {
                    _logger.LogWarning("No container id available. Fallback to use the first container for status checking.");
                    myContainerId = myPod.GetAllContainerStatus().First().ContainerID;
                }

                ContainerStatus? status = myPod.GetContainerStatus(myContainerId);
                if (status != null && status.Ready)
                {
                    stopwatch.Stop();
                    _logger.LogDebug(Invariant($"K8s info available in: {stopwatch.ElapsedMilliseconds} ms."));
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
    }
}
