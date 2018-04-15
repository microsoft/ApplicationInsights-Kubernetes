using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpClient : HttpClient, IKubeHttpClient
    {
        public IKubeHttpClientSettingsProvider Settings { get; private set; }

        public KubeHttpClient(
            IKubeHttpClientSettingsProvider settingsProvider,
            ILogger<KubeHttpClient> logger)
            : base(settingsProvider.CreateMessageHandler())
        {
            _logger = Arguments.IsNotNull(logger, nameof(logger));
            this.Settings = settingsProvider;
            string token = settingsProvider.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug($"Access token is not null. Set default request header.");
                this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("Access token is null.");
            }

            this.BaseAddress = settingsProvider.ServiceBaseAddress;
        }

        private readonly ILogger _logger;
    }
}
