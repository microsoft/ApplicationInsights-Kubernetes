using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sQueryClientFactory
    {
        private readonly Logger _logger = Logger.Instance;

        public K8sQueryClient Create(IKubeHttpClient httpClient)
        {
            _logger.LogTrace("Creating {0}", nameof(K8sQueryClient));
            return new K8sQueryClient(httpClient);
        }
    }
}
