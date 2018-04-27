using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpClientFactory
    {
        public KubeHttpClientFactory(ILogger<KubeHttpClientFactory> logger, ILoggerFactory loggerFactory)
        {
            _logger = Arguments.IsNotNull(logger, nameof(logger));
            _loggerFactory = Arguments.IsNotNull(loggerFactory, nameof(loggerFactory));
        }

        public IKubeHttpClient Create(IKubeHttpClientSettingsProvider settingsProvider)
        {
            _logger.LogTrace($"Creating {nameof(KubeHttpClient)}");
            return new KubeHttpClient(settingsProvider, _loggerFactory.CreateLogger<KubeHttpClient>());
        }

        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
    }
}
