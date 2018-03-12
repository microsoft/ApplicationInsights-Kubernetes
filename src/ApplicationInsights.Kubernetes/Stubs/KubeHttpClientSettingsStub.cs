using System;
using System.Net.Http;

namespace Microsoft.ApplicationInsights.Kubernetes.Stubs
{
    internal class KubeHttpClientSettingsStub : IKubeHttpClientSettingsProvider
    {
        public const string FakeContainerId = "F8E1C6FF-2217-4962-90FF-0D9195AF0785";

        public string ContainerId => FakeContainerId;

        public string QueryNamespace => "063A30B8-6A62-4519-8BFE-0DE144B009A1";

        public Uri ServiceBaseAddress => new Uri("http://localhost/stub");

        public HttpMessageHandler CreateMessageHandler()
        {
            return null;
        }

        public string GetToken()
        {
            return null;
        }
    }
}
