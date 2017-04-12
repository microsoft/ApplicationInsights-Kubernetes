using System;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IKubeHttpClient
    {
        IKubeHttpClientSettingsProvider Settings { get; }

        Task<string> GetStringAsync(Uri requestUri);
    }
}