namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity;
    using Newtonsoft.Json;

    using static Microsoft.ApplicationInsights.Netcore.Kubernetes.StringUtils;

    /// <summary>
    /// High level query client for K8s concepts.
    /// </summary>
    internal class K8sQueryClient
    {
        KubeHttpClient kubeHttpClient;
        public K8sQueryClient(KubeHttpClient kubeHttpClient)
        {
            this.kubeHttpClient = kubeHttpClient ?? throw new ArgumentNullException(nameof(kubeHttpClient));
        }


        #region Pods
        /// <summary>
        /// Get all pods in this cluster
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Pod>> GetPodsAsync()
        {
            string url = Invariant($"api/v1/namespaces/{kubeHttpClient.Settings.QueryNamespace}/pods");
            Uri requestUri = new Uri(url);
            string resultString = await kubeHttpClient.GetStringAsync(requestUri).ConfigureAwait(false);
            PodList pods = JsonConvert.DeserializeObject<PodList>(resultString);
            return pods.Items;
        }

        /// <summary>
        /// Get the pod the current container is running upon.
        /// </summary>
        /// <returns></returns>
        public async Task<Pod> GetMyPodAsync()
        {
            string myContainerId = kubeHttpClient.Settings.ContainerId;
            IEnumerable<Pod> possiblePods = await GetPodsAsync().ConfigureAwait(false);
            Pod targetPod = possiblePods.FirstOrDefault(pod => pod.Status != null &&
                                pod.Status.ContainerStatuses != null &&
                                pod.Status.ContainerStatuses.Any(
                                    cs => !string.IsNullOrEmpty(cs.ContainerID) && cs.ContainerID.EndsWith(myContainerId, StringComparison.Ordinal)));
            return targetPod;
        }
        #endregion

        #region ContainerStatus
        /// <summary>
        /// Get the container status for the pod, where the current container is running upon.
        /// </summary>
        /// <returns></returns>
        public async Task<ContainerStatus> GetMyContainerStatusAsync()
        {
            string myContainerId = kubeHttpClient.Settings.ContainerId;
            Pod myPod = await GetMyPodAsync().ConfigureAwait(false);
            return myPod.GetContainerStatus(myContainerId);
        }
        #endregion
    }
}
