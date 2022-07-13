using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IKubeHttpClient : IDisposable
    {
        IKubeHttpClientSettingsProvider Settings { get; }

        Task<string> GetStringAsync(Uri requestUri);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

        HttpRequestHeaders DefaultRequestHeaders { get; }
    }
}
