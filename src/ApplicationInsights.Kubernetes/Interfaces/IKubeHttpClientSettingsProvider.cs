namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IKubeHttpClientSettingsProvider : IHttpClientSettingsProvider
    {
        string ContainerId { get; }
        string QueryNamespace { get; }

    }
}
