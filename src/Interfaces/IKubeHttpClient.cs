using System;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IKubeHttpClient : IDisposable
    {
        IKubeHttpClientSettingsProvider Settings { get; }

        Task<string> GetStringAsync(Uri requestUri);
    }
}