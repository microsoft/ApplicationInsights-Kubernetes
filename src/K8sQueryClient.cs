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
    internal class K8sQueryClient : IDisposable, IK8sQueryClient
    {
        internal bool disposed = false;
        private IKubeHttpClient kubeHttpClient;
        internal IKubeHttpClient KubeHttpClient
        {
            get
            {
                EnsureNotDisposed();
                return this.kubeHttpClient;
            }
        }

        public K8sQueryClient(IKubeHttpClient kubeHttpClient)
        {
            this.kubeHttpClient = Arguments.IsNotNull(kubeHttpClient, nameof(kubeHttpClient));
        }


        #region Pods
        /// <summary>
        /// Get all pods in this cluster
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<K8sPod>> GetPodsAsync()
        {
            EnsureNotDisposed();
            string url = Invariant($"api/v1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/pods");
            return GetAllItemsAsync<K8sPod>(url);
        }

        /// <summary>
        /// Get the pod the current container is running upon.
        /// </summary>
        /// <returns></returns>
        public async Task<K8sPod> GetMyPodAsync()
        {
            EnsureNotDisposed();

            string myContainerId = KubeHttpClient.Settings.ContainerId;
            IEnumerable<K8sPod> possiblePods = await GetPodsAsync().ConfigureAwait(false);
            if (possiblePods == null)
            {
                return null;
            }
            K8sPod targetPod = possiblePods.FirstOrDefault(pod => pod.Status != null &&
                                pod.Status.ContainerStatuses != null &&
                                pod.Status.ContainerStatuses.Any(
                                    cs => !string.IsNullOrEmpty(cs.ContainerID) && cs.ContainerID.EndsWith(myContainerId, StringComparison.Ordinal)));
            return targetPod;
        }
        #endregion

        #region Replica Sets
        public Task<IEnumerable<K8sReplicaSet>> GetReplicasAsync()
        {
            EnsureNotDisposed();

            string url = Invariant($"apis/extensions/v1beta1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/replicasets");
            return GetAllItemsAsync<K8sReplicaSet>(url);
        }
        #endregion

        #region Deployment
        public Task<IEnumerable<K8sDeployment>> GetDeploymentsAsync()
        {
            EnsureNotDisposed();

            string url = Invariant($"apis/extensions/v1beta1/namespaces/{this.KubeHttpClient.Settings.QueryNamespace}/deployments");
            return GetAllItemsAsync<K8sDeployment>(url);
        }
        #endregion

        #region Node
        public Task<IEnumerable<K8sNode>> GetNodesAsync()
        {
            EnsureNotDisposed();

            string url = Invariant($"api/v1/nodes");
            return GetAllItemsAsync<K8sNode>(url);
        }
        #endregion

        #region ContainerStatus
        /// <summary>
        /// Get the container status for the pod, where the current container is running upon.
        /// </summary>
        /// <returns></returns>
        public async Task<ContainerStatus> GetMyContainerStatusAsync()
        {
            EnsureNotDisposed();

            string myContainerId = KubeHttpClient.Settings.ContainerId;
            K8sPod myPod = await GetMyPodAsync().ConfigureAwait(false);
            return myPod.GetContainerStatus(myContainerId);
        }
        #endregion

        private async Task<IEnumerable<TEntity>> GetAllItemsAsync<TEntity>(string relativeUrl)
        {
            Uri requestUri = GetQueryUri(relativeUrl);
            string resultString = await this.KubeHttpClient.GetStringAsync(requestUri).ConfigureAwait(false);
            K8sEntityList<TEntity> resultList = JsonConvert.DeserializeObject<K8sEntityList<TEntity>>(resultString);
            return resultList.Items;
        }

        private Uri GetQueryUri(string relativeUrl)
        {
            return new Uri(this.KubeHttpClient.Settings.ServiceBaseAddress, relativeUrl);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }
            this.disposed = true;
            if (disposing)
            {
                if (this.kubeHttpClient != null)
                {
                    this.kubeHttpClient.Dispose();
                    this.kubeHttpClient = null;
                }
            }
        }

        private void EnsureNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(kubeHttpClient));
            }
        }
    }
}
