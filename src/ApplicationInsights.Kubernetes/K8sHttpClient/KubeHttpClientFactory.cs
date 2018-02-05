using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpClientFactory
    {
        private readonly ILogger _logger;
        public KubeHttpClientFactory(ILogger<KubeHttpClientFactory> logger)
        {
            _logger = Arguments.IsNotNull(logger, nameof(logger));
        }

        public IKubeHttpClient Create(IKubeHttpClientSettingsProvider settingsProvider)
        {
            _logger.LogTrace($"Creating {nameof(KubeHttpClient)}");
            return new KubeHttpClient(settingsProvider);
        }
    }
}
