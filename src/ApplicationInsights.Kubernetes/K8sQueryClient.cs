#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
        internal bool _disposed = false;
        private readonly IKubeHttpClient _kubeHttpClient;
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        internal IKubeHttpClient KubeHttpClient
        {
            get
            {
                EnsureNotDisposed();
                return _kubeHttpClient;
            }
        }

        public K8sQueryClient(IKubeHttpClient kubeHttpClient)
        {
            _kubeHttpClient = Arguments.IsNotNull(kubeHttpClient, nameof(kubeHttpClient));
        }


        #region Pods
        /// <summary>
        /// Gets all pods in this cluster
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<K8sPod>> GetPodsAsync(CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            string url = Invariant($"api/v1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/pods");
            return GetAllItemsAsync<K8sPod>(url, cancellationToken);
        }

        /// <summary>
        /// Gets a pod by name
        /// </summary>
        /// <param name="podName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<K8sPod?> GetPodAsync(string podName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(podName))
            {
                throw new ArgumentException($"'{nameof(podName)}' cannot be null or whitespace.", nameof(podName));
            }

            EnsureNotDisposed();
            string url = Invariant($"api/v1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/pods/{podName}");

            return GetItemAsync<K8sPod>(url, cancellationToken);
        }
        #endregion

        #region Replica Sets
        public Task<IEnumerable<K8sReplicaSet>> GetReplicasAsync(CancellationToken cancellationToken)
        {
            EnsureNotDisposed();

            string url = Invariant($"apis/apps/v1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/replicasets");
            return GetAllItemsAsync<K8sReplicaSet>(url, cancellationToken);
        }
        #endregion

        #region Deployment
        public Task<IEnumerable<K8sDeployment>> GetDeploymentsAsync(CancellationToken cancellationToken)
        {
            EnsureNotDisposed();

            string url = Invariant($"apis/apps/v1/namespaces/{KubeHttpClient.Settings.QueryNamespace}/deployments");
            return GetAllItemsAsync<K8sDeployment>(url, cancellationToken);
        }
        #endregion

        #region Node
        public Task<IEnumerable<K8sNode>> GetNodesAsync(CancellationToken cancellationToken)
        {
            EnsureNotDisposed();

            string url = Invariant($"api/v1/nodes");
            return GetAllItemsAsync<K8sNode>(url, cancellationToken);
        }
        #endregion

        private async Task<IEnumerable<TEntity>> GetAllItemsAsync<TEntity>(string relativeUrl, CancellationToken cancellationToken)
        {
            Uri requestUri = GetQueryUri(relativeUrl);
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            _logger.LogTrace("Default Header: {0}", KubeHttpClient.DefaultRequestHeaders);

            using HttpResponseMessage response = await KubeHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogTrace("Query succeeded.");
                string resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                K8sEntityList<TEntity>? resultList = JsonConvert.DeserializeObject<K8sEntityList<TEntity>>(resultString);
                return resultList?.Items ?? Enumerable.Empty<TEntity>();
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

        private async Task<TEntity?> GetItemAsync<TEntity>(string relativeUrl, CancellationToken cancellationToken)
        {
            Uri requestUri = GetQueryUri(relativeUrl);
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            _logger.LogTrace("Default Header: {0}", KubeHttpClient.DefaultRequestHeaders);

            using HttpResponseMessage response = await KubeHttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogTrace("Query succeeded.");
                string resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                TEntity? result = JsonConvert.DeserializeObject<TEntity>(resultString);
                return result;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(response.ReasonPhrase);
            }
            else
            {
                _logger.LogDebug("Query Failed. Request Message: {0}. Status Code: {1}. Phase: {2}", response.RequestMessage, response.StatusCode, response.ReasonPhrase);
                return default;
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
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            if (disposing)
            {
                if (_kubeHttpClient != null)
                {
                    _kubeHttpClient.Dispose();
                }
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(_kubeHttpClient));
            }
        }
    }
}
