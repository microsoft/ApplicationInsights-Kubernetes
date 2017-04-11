namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.Kubernetes.Entities;
    using Newtonsoft.Json;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

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
        public Task<IEnumerable<K8sPod>> GetPodsAsync()
        {
            string url = Invariant($"api/v1/namespaces/{kubeHttpClient.Settings.QueryNamespace}/pods");
            return GetAllItemsAsync<K8sPod, K8sPodList>(url);
        }

        /// <summary>
        /// Get the pod the current container is running upon.
        /// </summary>
        /// <returns></returns>
        public async Task<K8sPod> GetMyPodAsync()
        {
            string myContainerId = kubeHttpClient.Settings.ContainerId;
            IEnumerable<K8sPod> possiblePods = await GetPodsAsync().ConfigureAwait(false);
            K8sPod targetPod = possiblePods.FirstOrDefault(pod => pod.Status != null &&
                                pod.Status.ContainerStatuses != null &&
                                pod.Status.ContainerStatuses.Any(
                                    cs => !string.IsNullOrEmpty(cs.ContainerID) && cs.ContainerID.EndsWith(myContainerId, StringComparison.Ordinal)));
            return targetPod;
        }
        #endregion

        #region Replica Sets
        public Task<IEnumerable<ReplicaSet>> GetReplicasAsync()
        {
            string url = Invariant($"apis/extensions/v1beta1/namespaces/{kubeHttpClient.Settings.QueryNamespace}/replicasets");
            return GetAllItemsAsync<ReplicaSet, ReplicaSetList>(url);
        }
        #endregion

        #region Deployment
        public Task<IEnumerable<K8sDeployment>> GetDeploymentsAsync()
        {
            string url = Invariant($"apis/extensions/v1beta1/namespaces/{this.kubeHttpClient.Settings.QueryNamespace}/deployments");
            return GetAllItemsAsync<K8sDeployment, K8sDeploymentList>(url);
        }
        #endregion

        #region Node
        public Task<IEnumerable<K8sNode>> GetNodesAsync()
        {
            string url = Invariant($"api/v1/nodes");
            return GetAllItemsAsync<K8sNode, K8sNodeList>(url);
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
            K8sPod myPod = await GetMyPodAsync().ConfigureAwait(false);
            return myPod.GetContainerStatus(myContainerId);
        }
        #endregion

        private async Task<IEnumerable<TEntity>> GetAllItemsAsync<TEntity, TList>(string relativeUrl)
            where TList : K8sObjectList<TEntity>
        {
            Uri requestUri = GetQueryUri(relativeUrl);
            string resultString = await this.kubeHttpClient.GetStringAsync(requestUri).ConfigureAwait(false);
            TList resultList = JsonConvert.DeserializeObject<TList>(resultString);
            return resultList.Items;
        }

        private Uri GetQueryUri(string relativeUrl)
        {
            return new Uri(this.kubeHttpClient.Settings.ServiceBaseAddress, relativeUrl);
        }
    }
}
