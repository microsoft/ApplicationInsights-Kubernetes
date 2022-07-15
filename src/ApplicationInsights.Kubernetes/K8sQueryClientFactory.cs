using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sQueryClientFactory : IK8sQueryClientFactory
    {
        private readonly KubeHttpClientFactory _kubeHttpClientFactory;
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
        
        public K8sQueryClientFactory(KubeHttpClientFactory kubeHttpClientFactory)
        {
            _kubeHttpClientFactory = kubeHttpClientFactory ?? throw new System.ArgumentNullException(nameof(kubeHttpClientFactory));
        }

        public IK8sQueryClient Create()
        {
            _logger.LogTrace("Creating {0}", nameof(K8sQueryClient));
            return new K8sQueryClient(_kubeHttpClientFactory.Create());
        }
    }
}
