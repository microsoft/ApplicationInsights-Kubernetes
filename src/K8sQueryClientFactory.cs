using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sQueryClientFactory
    {
        private readonly ILogger _logger;

        public K8sQueryClientFactory(ILogger<K8sQueryClientFactory> logger)
        {
            _logger = Arguments.IsNotNull(logger, nameof(logger));
        }

        public K8sQueryClient Create(IKubeHttpClient httpClient)
        {
            _logger.LogTrace($"Creating {nameof(K8sQueryClient)}");
            return new K8sQueryClient(httpClient);
        }
    }
}
