using System;
using System.Net.Http;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class KubeHttpClientTests
    {
        [Fact(DisplayName = "KueHttpClient ctor should set Settings property")]
        public void CtorShouldSetSettingsProperty()
        {
            var settingsMock = new Mock<IKubeHttpClientSettingsProvider>();
            settingsMock.Setup(p => p.CreateMessageHandler()).Returns(new HttpClientHandler());
            KubeHttpClient client = new KubeHttpClient(settingsMock.Object);

            Assert.NotNull(client.Settings);
            Assert.Equal(settingsMock.Object, client.Settings);
        }

        [Fact(DisplayName = "KubeHttpClient create message handler should not return null")]
        public void CreateMessageHandlerShouldNotReturnNull()
        {
            var settingsMock = new Mock<IKubeHttpClientSettingsProvider>();
            settingsMock.Setup(p => p.CreateMessageHandler()).Returns(() => null);

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            {
                KubeHttpClient client = new KubeHttpClient(settingsMock.Object);
            });

            Assert.Equal("handler", ex.ParamName);
        }

        [Fact(DisplayName = "KubeHttpClient ctor should set the BaseAddress property")]
        public void CtorShouldSetTheBaseAddress()
        {

            var settingsMock = new Mock<IKubeHttpClientSettingsProvider>();
            settingsMock.Setup(p => p.CreateMessageHandler()).Returns(new HttpClientHandler());
            Uri targetBaseUri = new Uri("https://k8stest/");
            settingsMock.Setup(p => p.ServiceBaseAddress).Returns(targetBaseUri);

            KubeHttpClient client = new KubeHttpClient(settingsMock.Object);

            Assert.NotNull(client.BaseAddress);
            Assert.Equal(targetBaseUri, client.BaseAddress);
        }
    }
}
