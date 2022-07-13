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

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should use pod name provided by {nameof(IPodNameProvider)} first when possible")]
    public async Task GetMyPodAsyncShouldLeveragePodNameProviders()
    {
        var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

        var httpClientMock = new Mock<IKubeHttpClient>();
        httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);

        var podNameProviderMock = new Mock<IPodNameProvider>();
        string providerPodName = "AnotherValidPod";
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(true); // the provider returns true with pod name.

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>
            {
                Items = new List<K8sPod>
                    {
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = "noisy" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy in front" } } }},
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = "another noise" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="containerId" } } }},
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = providerPodName}, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy after" } } }},
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
            Assert.Equal("noisy after", result.Status.ContainerStatuses.First().ContainerID); // When pod name is provided directly, do not use container id for matching.
            Assert.Equal(providerPodName, result.Metadata?.Name);
        }
    }

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should fallback to use container id when the name provided by {nameof(IPodNameProvider)} have no hit.")]
    public async Task GetMyPodAsyncShouldFallbackToUseContainerIdWhenProvidedPodNameNotMatch()
    {
        var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

        var httpClientMock = new Mock<IKubeHttpClient>();
        httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);

        var podNameProviderMock = new Mock<IPodNameProvider>();
        string providerPodName = "AnotherValidPod";
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(true); // the provider returns true with pod name.

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>
            {
                Items = new List<K8sPod>
                    {
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = "noisy" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy in front" } } }},
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = "not another noise" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="containerId" } } }},
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = "no matched pod name"}, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy after" } } }},
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
            // Provided pod name is not good, used the container id to get the target pod
            Assert.Equal("containerId", result.Status.ContainerStatuses.First().ContainerID);
            Assert.Equal("not another noise", result.Metadata?.Name);
        }
    }

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should support multiple {nameof(IPodNameProvider)} instances.")]
    public async Task GetMyPodAsyncShouldSupportMultipleIPodNameProviders()
    {
        var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

        var httpClientMock = new Mock<IKubeHttpClient>();
        httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);

        var podNameProviderMock = new Mock<IPodNameProvider>();
        var podNameProviderMock2 = new Mock<IPodNameProvider>();

        string providerPodName = string.Empty;
        string providerPodName2 = "AnotherValidPod";

        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(false); // the provider returns true with pod name.
        podNameProviderMock2.Setup(p => p.TryGetPodName(out providerPodName2)).Returns(true); // the provider returns true with pod name.


        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>
            {
                Items = new List<K8sPod>
                    {
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = "noisy" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy in front" }}}},
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = "hello" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="containerId" }}}},
                        new K8sPod(){ Metadata = new K8sPodMetadata { Name = providerPodName2 }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="matched by pod name provider" }}}},
                    }
            }))
        };
        httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>())).Returns(Task.FromResult(response));

        using (K8sQueryClient k8SQueryClient = new K8sQueryClient(httpClientMock.Object))
        {
            PodInfoManager target = new PodInfoManager(k8SQueryClient, httpClientSettingsMock.Object, new IPodNameProvider[] { podNameProviderMock.Object, podNameProviderMock2.Object });

            K8sPod result = await target.GetMyPodAsync(cancellationToken: default);

            Assert.NotNull(result);
            Assert.Single(result.Status.ContainerStatuses);
            Assert.Equal("matched by pod name provider", result.Status.ContainerStatuses.First().ContainerID);
            Assert.Equal(providerPodName2, result.Metadata?.Name);
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
