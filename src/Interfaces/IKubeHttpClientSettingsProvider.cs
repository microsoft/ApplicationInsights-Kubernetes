namespace Microsoft.ApplicationInsights.Kubernetes
{
    public interface IKubeHttpClientSettingsProvider : IHttpClientSettingsProvider
    {
        string ContainerId { get; }
        string QueryNamespace { get; }
    }
}
