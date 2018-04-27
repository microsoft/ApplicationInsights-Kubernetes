using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Microsoft.Extensions.Logging;

using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sEnvironmentFactory : IK8sEnvironmentFactory
    {
        private readonly ILogger _logger;
        private readonly IKubeHttpClientSettingsProvider _httpClientSettings;
        private readonly KubeHttpClientFactory _httpClientFactory;
        private readonly K8sQueryClientFactory _k8sQueryClientFactory;

        public K8sEnvironmentFactory(
            IKubeHttpClientSettingsProvider httpClientSettingsProvider,
            KubeHttpClientFactory httpClientFactory,
            K8sQueryClientFactory k8SQueryClientFactory,
            ILogger<K8sEnvironmentFactory> logger)
        {
            _logger = Arguments.IsNotNull(logger, nameof(logger));
            _httpClientSettings = Arguments.IsNotNull(httpClientSettingsProvider, nameof(httpClientSettingsProvider));
            _httpClientFactory = Arguments.IsNotNull(httpClientFactory, nameof(httpClientFactory));
            _k8sQueryClientFactory = Arguments.IsNotNull(k8SQueryClientFactory, nameof(k8SQueryClientFactory));
        }

        /// <summary>
        /// Async factory method to build the instance of a K8sEnvironment.
        /// </summary>
        /// <returns></returns>
        public async Task<IK8sEnvironment> CreateAsync(DateTime timeoutAt)
        {
            K8sEnvironment instance = null;

            try
            {
                using (IKubeHttpClient httpClient = _httpClientFactory.Create(_httpClientSettings))
                using (K8sQueryClient queryClient = _k8sQueryClientFactory.Create(httpClient))
                {
                    // TODO: See if there's better way to fetch the container id
                    K8sPod myPod = await SpinWaitUntilGetPod(timeoutAt, queryClient).ConfigureAwait(false);
                    if (myPod != null)
                    {
                        string containerId = null;
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            // For Windows, is there a way to fetch the current container id from within the container?
                            containerId = myPod.Status.ContainerStatuses.First().ContainerID;
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            // For Linux, container id could be fetched directly from cGroup.
                            containerId = _httpClientSettings.ContainerId;
                        }
                        // ~

                        if (await SpinWaitContainerReady(timeoutAt, queryClient, containerId).ConfigureAwait(false))
                        {
                            instance = new K8sEnvironment()
                            {
                                ContainerID = containerId
                            };

                            instance.myPod = myPod;
                            _logger.LogDebug(Invariant($"Getting container status of container-id: {containerId}"));
                            instance.myContainerStatus = myPod.GetContainerStatus(containerId);

                            IEnumerable<K8sReplicaSet> replicaSetList = await queryClient.GetReplicasAsync().ConfigureAwait(false);
                            instance.myReplicaSet = myPod.GetMyReplicaSet(replicaSetList);

                            if (instance.myReplicaSet != null)
                            {
                                IEnumerable<K8sDeployment> deploymentList = await queryClient.GetDeploymentsAsync().ConfigureAwait(false);
                                instance.myDeployment = instance.myReplicaSet.GetMyDeployment(deploymentList);
                            }

                            if (instance.myPod != null)
                            {
                                IEnumerable<K8sNode> nodeList = await queryClient.GetNodesAsync().ConfigureAwait(false);
                                string nodeName = instance.myPod.Spec.NodeName;
                                if (!string.IsNullOrEmpty(nodeName))
                                {
                                    instance.myNode = nodeList.FirstOrDefault(node => !string.IsNullOrEmpty(node.Metadata?.Name) && node.Metadata.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError(Invariant($"Kubernetes info is not available before the timeout at {timeoutAt}."));
                        }
                    }
                    else
                    {
                        // MyPod is null, meaning query timed out.
                        _logger.LogCritical("Fail to fetch the pod information in time. Kubernetes info will not be available for the telemetry.");
                        return null;
                    }
                }
                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
                return null;
            }
        }

        private async Task<K8sPod> SpinWaitUntilGetPod(DateTime timeoutAt, K8sQueryClient client)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            K8sPod myPod = null;
            do
            {
                // When my pod become available and it's status become ready, we recognize the container is ready.
                try
                {
                    myPod = await client.GetMyPodAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Query exception while trying to get pod info: {ex.Message}");
                    _logger.LogDebug(ex.StackTrace);
                    myPod = null;
                }

                if (myPod != null)
                {
                    stopwatch.Stop();
                    _logger.LogDebug(Invariant($"K8s info avaialbe in: {stopwatch.ElapsedMilliseconds} ms."));
                    return myPod;
                }

                // The time to get the container ready dependes on how much time will a container to be initialized.
                // When there is readiness probe, the pod info will not be available until the initial delay of it is elapsed.
                // When there is no readiness probe, the minimum seems about 1000ms. 
                // Try invoke a probe on readiness every 500ms until the container is ready
                // Or it will timeout per the timeout settings.
                await Task.Delay(500).ConfigureAwait(false);
            } while (DateTime.Now < timeoutAt);
            return null;
        }

        /// <summary>
        /// Wait until the container is ready.
        /// Refer document @ https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-phase for the Pod's lifecycle.
        /// </summary>
        /// <param name="timeout">Timeout on Application Insights data when the container is not ready after the period.</param>
        /// <param name="client">Query client to try getting info from the Kubernetes cluster API.</param>
        /// <param name="myContainerId">The container that we are interested in.</param>
        /// <returns></returns>
        private async Task<bool> SpinWaitContainerReady(DateTime timeoutAt, K8sQueryClient client, string myContainerId)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            K8sPod myPod = null;
            do
            {
                // When my pod become available and it's status become ready, we recognize the container is ready.
                try
                {
                    myPod = await client.GetMyPodAsync().ConfigureAwait(false);
                }
                catch
                {
                    myPod = null;
                }

                if (myPod != null && myPod.GetContainerStatus(myContainerId).Ready)
                {
                    stopwatch.Stop();
                    _logger.LogDebug(Invariant($"K8s info avaialbe in: {stopwatch.ElapsedMilliseconds} ms."));
                    return true;
                }

                // The time to get the container ready dependes on how much time will a container to be initialized.
                // When there is readiness probe, the pod info will not be available until the initial delay of it is elapsed.
                // When there is no readiness probe, the minimum seems about 1000ms. 
                // Try invoke a probe on readiness every 500ms until the container is ready
                // Or it will timeout per the timeout settings.
                await Task.Delay(500).ConfigureAwait(false);
            } while (DateTime.Now < timeoutAt);
            return false;
        }
    }
}
