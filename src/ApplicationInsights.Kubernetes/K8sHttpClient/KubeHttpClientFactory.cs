using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpClientFactory
    {
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
        private readonly IKubeHttpClientSettingsProvider _settingsProvider;

        public KubeHttpClientFactory(IKubeHttpClientSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider ?? throw new System.ArgumentNullException(nameof(settingsProvider));
        }

        public IKubeHttpClient Create()
        {
            _logger.LogTrace("Creating {0}", nameof(KubeHttpClient));
            return new KubeHttpClient(_settingsProvider);
        }
    }
}
