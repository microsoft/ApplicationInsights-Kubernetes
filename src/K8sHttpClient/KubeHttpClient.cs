namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System.Net.Http;
    using System.Net.Http.Headers;

    /// <summary>
    /// This is the low-level http client wrapper to setup a http client specificly working for Kubernetes.
    /// This purpose of this client is to setup basics like the base address, the bearer token and so on that 
    /// the high-level query object doens't need to deal with these common settings.
    /// </summary>
    internal class KubeHttpClient : HttpClient, IKubeHttpClient
    {
        /// <summary>
        /// Gets or sets the settings for the httpclient for kubernetes restful calls.
        /// </summary>
        public IKubeHttpClientSettingsProvider Settings { get; private set; }

        /// <summary>
        /// Create a KubeHttpClient object.
        /// </summary>
        /// <param name="settingsProvider">Setting provider. Refer <see cref="IKubeHttpClientSettingsProvider"/>.</param>
        public KubeHttpClient(IKubeHttpClientSettingsProvider settingsProvider) : base(settingsProvider.CreateMessageHandler())
        {
            this.Settings = settingsProvider;
            string token = settingsProvider.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                this.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            this.BaseAddress = settingsProvider.ServiceBaseAddress;
        }
    }
}
