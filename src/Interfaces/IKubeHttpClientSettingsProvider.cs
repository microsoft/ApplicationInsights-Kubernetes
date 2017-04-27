namespace Microsoft.ApplicationInsights.Kubernetes
{
    /// <summary>
    /// Interface that provides Kubernetes related settings.
    /// </summary>
    internal interface IKubeHttpClientSettingsProvider : IHttpClientSettingsProvider
    {
        /// <summary>
        /// Gets the container id.
        /// </summary>
        string ContainerId { get; }

        /// <summary>
        /// Gets the namespace for Kubernetes http queries.
        /// </summary>
        string QueryNamespace { get; }
    }
}
