using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Newtonsoft.Json;
using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.ApplicationInsights.Kubernetes
{

    /// <summary>
    /// High level query client for K8s concepts.
    /// </summary>
    internal class K8sQueryClient : IDisposable, IK8sQueryClient
    {
        internal bool disposed = false;
        private IKubeHttpClient _kubeHttpClient;
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        internal IKubeHttpClient KubeHttpClient
        {
            get
            {
                EnsureNotDisposed();
                return this._kubeHttpClient;
            }
        }

        public K8sQueryClient(IKubeHttpClient kubeHttpClient)
        {
            this._kubeHttpClient = Arguments.IsNotNull(kubeHttpClient, nameof(kubeHttpClient));
        }


        #region Pods
        /// <summary>
        /// Gets all pods in this cluster
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<K8sPod>> GetPodsAsync()
        {
            EnsureNotDisposed();
            string url = Invariant($"api/v1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/pods");
            return GetAllItemsAsync<K8sPod>(url);
        }
        #endregion

        #region Replica Sets
        public Task<IEnumerable<K8sReplicaSet>> GetReplicasAsync()
        {
            EnsureNotDisposed();

            string url = Invariant($"apis/apps/v1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/replicasets");
            return GetAllItemsAsync<K8sReplicaSet>(url);
        }
        #endregion

        #region Deployment
        public Task<IEnumerable<K8sDeployment>> GetDeploymentsAsync()
        {
            EnsureNotDisposed();

            string url = Invariant($"apis/apps/v1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/deployments");
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

        private async Task<IEnumerable<TEntity>> GetAllItemsAsync<TEntity>(string relativeUrl)
        {
            Uri requestUri = GetQueryUri(relativeUrl);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            _logger.LogTrace("Default Header: {0}", KubeHttpClient.DefaultRequestHeaders);

            HttpResponseMessage response = await KubeHttpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Query succeeded.");
                string resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                K8sEntityList<TEntity> resultList = JsonConvert.DeserializeObject<K8sEntityList<TEntity>>(resultString);
                return resultList.Items ?? Enumerable.Empty<TEntity>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(response.ReasonPhrase);
            }
            else
            {
                _logger.LogDebug("Query Failed. Request Message: {0}. Status Code: {1}. Phase: {2}", response.RequestMessage, response.StatusCode, response.ReasonPhrase);
                return Enumerable.Empty<TEntity>();
            }
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
                if (this._kubeHttpClient != null)
                {
                    this._kubeHttpClient.Dispose();
                    this._kubeHttpClient = null;
                }
            }
        }

        private void EnsureNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(_kubeHttpClient));
            }
        }
    }
}
