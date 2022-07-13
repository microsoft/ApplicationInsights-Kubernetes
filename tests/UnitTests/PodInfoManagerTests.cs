using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;

public class PodInfoManagerTests
{
    [Fact(DisplayName = "GetMyPodsAsync should request the target uri")]
    public async Task GetMyPodAsyncHitsTheUri()
    {
        var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

        var httpClientMock = new Mock<IKubeHttpClient>();
        httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>()))
        };

        httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>())).Returns(Task.FromResult(
            response));

        var podNameProviderMock = new Mock<IPodNameProvider>();

        using (K8sQueryClient k8SQueryClient = new K8sQueryClient(httpClientMock.Object))
        {
            PodInfoManager target = new PodInfoManager(k8SQueryClient, httpClientSettingsMock.Object, new IPodNameProvider[] { podNameProviderMock.Object });
            await target.GetMyPodAsync(cancellationToken: default);
        }

        httpClientMock.Verify(mock => mock.SendAsync(It.Is<HttpRequestMessage>(
            m => m.RequestUri.AbsoluteUri.Equals("https://baseaddress/api/v1/namespaces/queryNamespace/pods"))), Times.Once);
    }

    [Fact(DisplayName = "GetPodAsync should get my pod correctly")]
    public async Task GetMyPodAsyncShouldGetCorrectPod()
    {
        var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

        var httpClientMock = new Mock<IKubeHttpClient>();
        httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);

        var podNameProviderMock = new Mock<IPodNameProvider>();

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>
            {
                Items = new List<K8sPod>
                    {
                        new K8sPod(){ Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy in front" } } }},
                        new K8sPod(){ Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="containerId" } } }},
                        new K8sPod(){ Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy after" } } }}
                    }
            }))
        };
        httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>())).Returns(Task.FromResult(response));

        using (K8sQueryClient k8SQueryClient = new K8sQueryClient(httpClientMock.Object))
        {
            PodInfoManager target = new PodInfoManager(k8SQueryClient, httpClientSettingsMock.Object, new IPodNameProvider[] { podNameProviderMock.Object });

            K8sPod result = await target.GetMyPodAsync(cancellationToken: default);

            Assert.NotNull(result);
            Assert.Single(result.Status.ContainerStatuses);
            Assert.Equal("containerId", result.Status.ContainerStatuses.First().ContainerID);
        }
    }

    private Mock<IKubeHttpClientSettingsProvider> GetKubeHttpClientSettingsProviderForTest()
    {
        Uri baseUri = new Uri("https://baseAddress/");
        string queryNamespace = nameof(queryNamespace);
        string containerId = nameof(containerId);

        var httpClientSettings = new Mock<IKubeHttpClientSettingsProvider>();
        httpClientSettings.Setup(settings => settings.ServiceBaseAddress).Returns(baseUri);
        httpClientSettings.Setup(settings => settings.QueryNamespace).Returns(queryNamespace);
        httpClientSettings.Setup(settings => settings.ContainerId).Returns(containerId);

        return httpClientSettings;
    }
}
