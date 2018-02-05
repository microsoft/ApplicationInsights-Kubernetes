namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Net.Http;

    internal interface IHttpClientSettingsProvider
    {
        Uri ServiceBaseAddress { get; }

        HttpMessageHandler CreateMessageHandler();

        string GetToken();
    }
}