using System;
using System.Net.Http;
using Microsoft.ApplicationInsights.Kubernetes;

namespace ApplicationInsights.Kubernetes.Samples.Watcher
{
    internal class KubectlProxySettingsProvider : IHttpClientSettingsProvider, IKubeHttpClientSettingsProvider
    {
        private string _proxyUrl;
        private string _namespace;
        private string _containerId;
        public KubectlProxySettingsProvider(string proxyUrl,
            string queryNamespace,
            string containerId)
        {
            _proxyUrl = proxyUrl ?? "http://localhost:8001/";
            _namespace = queryNamespace ?? "default";

            if (string.IsNullOrEmpty(containerId))
            {
                throw new ArgumentNullException("Container Id has to be specified!");
            }
            _containerId = containerId;
        }

        public Uri ServiceBaseAddress => new Uri(_proxyUrl);

        public string ContainerId => _containerId;

        public string QueryNamespace => _namespace;

        public HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler();
        }

        public string GetToken()
        {
            return string.Empty;
        }
    }
}
