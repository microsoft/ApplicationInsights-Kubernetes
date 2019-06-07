using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpClient : HttpClient, IKubeHttpClient
    {
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
        public IKubeHttpClientSettingsProvider Settings { get; private set; }

        public KubeHttpClient(
            IKubeHttpClientSettingsProvider settingsProvider)
            : base(settingsProvider.CreateMessageHandler())
        {
            this.Settings = settingsProvider;
            string token = settingsProvider.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("Access token is not null. Set default request header.");
                this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("Access token is null.");
            }

            this.BaseAddress = settingsProvider.ServiceBaseAddress;
        }

    }
}
