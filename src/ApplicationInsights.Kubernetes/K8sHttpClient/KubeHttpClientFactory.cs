using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpClientFactory
    {
        private readonly Logger _logger = Logger.Instance;

        public KubeHttpClientFactory()
        {
        }

        public IKubeHttpClient Create(IKubeHttpClientSettingsProvider settingsProvider)
        {
            _logger.LogTrace("Creating {0}", nameof(KubeHttpClient));
            return new KubeHttpClient(settingsProvider);
        }
    }
}
