using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sQueryClientFactory
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public K8sQueryClientFactory(ILogger<K8sQueryClientFactory> logger, ILoggerFactory loggerFactory)
        {
            _logger = Arguments.IsNotNull(logger, nameof(logger));
            _loggerFactory = Arguments.IsNotNull(loggerFactory, nameof(loggerFactory));
        }

        public K8sQueryClient Create(IKubeHttpClient httpClient)
        {
            _logger.LogTrace($"Creating {nameof(K8sQueryClient)}");
            return new K8sQueryClient(httpClient, _loggerFactory.CreateLogger<K8sQueryClient>());
        }
    }
}
