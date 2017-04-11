namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    public interface IKubeHttpClientSettingsProvider : IHttpClientSettingsProvider
    {
        string ContainerId { get; }
        string QueryNamespace { get; }
    }
}
