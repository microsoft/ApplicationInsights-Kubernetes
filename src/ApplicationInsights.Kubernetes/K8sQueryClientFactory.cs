using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sQueryClientFactory
    {
        private readonly ILogger _logger;
        private readonly ILogger<K8sQueryClient> _queryClientLogger;

        public K8sQueryClientFactory(ILogger<K8sQueryClientFactory> logger, ILogger<K8sQueryClient> queryClientLogger)
        {
            _logger = Arguments.IsNotNull(logger, nameof(logger));
            _queryClientLogger = Arguments.IsNotNull(queryClientLogger, nameof(queryClientLogger));
        }

        public K8sQueryClient Create(IKubeHttpClient httpClient)
        {
            _logger.LogTrace($"Creating {nameof(K8sQueryClient)}");
            return new K8sQueryClient(httpClient, _queryClientLogger);
        }
    }
}
