using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IKubeHttpClient : IDisposable
    {
        IKubeHttpClientSettingsProvider Settings { get; }

        /// <summary>
        /// Sends a GET request to the specified Uri and return the response body as a string
        /// in an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">Request Uri.</param>
        /// <returns>Response body as a string.</returns>
        Task<string> GetStringAsync(Uri requestUri);

        /// <summary>
        /// Sends a GET request to the specified Uri and return the response body as a stream
        /// in an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">Request Uri.</param>
        /// <returns>Response body as a stream.</returns>
        Task<Stream> GetStreamAsync(Uri requestUri);

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="relativePath">Passes in the relative path.</param>
        /// <returns>The full path for a restful query.</returns>
        Uri GetQueryUrl(string relativePath);
    }
}