namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Net.Http;

    public interface IHttpClientSettingsProvider
    {
        Uri ServiceBaseAddress { get; }

        HttpMessageHandler CreateMessageHandler();

        string GetToken();
    }
}