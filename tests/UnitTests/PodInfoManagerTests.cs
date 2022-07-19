using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;

public class PodInfoManagerTests
{
    [Fact(DisplayName = "GetPodAsync should get my pod correctly")]
    public async Task GetMyPodAsyncShouldGetCorrectPod()
    {
        Mock<IK8sQueryClient> k8sQueryClientMock = new();
        Mock<IPodNameProvider> podNameProviderMock = new();
        Mock<IK8sQueryClientFactory> k8sQueryClientFactoryMock = new Mock<IK8sQueryClientFactory>();

        k8sQueryClientFactoryMock.Setup(f => f.Create()).Returns(k8sQueryClientMock.Object);
        k8sQueryClientMock.Setup(c => c.GetPodsAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult<IEnumerable<K8sPod>>(
            new K8sPod[] {
                new K8sPod(){ Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy in front" } } }},
                new K8sPod(){ Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="containerId" } } }},
                new K8sPod(){ Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy after" } } }},
            }
        ));

        PodInfoManager target = new PodInfoManager(k8sQueryClientFactoryMock.Object, GetKubeHttpClientSettingsProviderForTest().Object, new IPodNameProvider[] { podNameProviderMock.Object });
        K8sPod result = await target.GetMyPodAsync(default).ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Single(result.Status.ContainerStatuses);
        Assert.Equal("containerId", result.Status.ContainerStatuses.First().ContainerID);
    }

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should use pod name provided by {nameof(IPodNameProvider)} first when possible")]
    public async Task GetMyPodAsyncShouldLeveragePodNameProviders()
    {
        string providerPodName = "podNameByProvider";
        Mock<IK8sQueryClient> k8sQueryClientMock = new();
        Mock<IPodNameProvider> podNameProviderMock = new();
        Mock<IK8sQueryClientFactory> k8sQueryClientFactoryMock = new Mock<IK8sQueryClientFactory>();
        K8sPod[] podsArray = new[]{
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = "noisy" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy in front" } } }},
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = "another noise" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="containerId" } } }},
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = providerPodName}, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy after" } } }},
        };

        k8sQueryClientFactoryMock.Setup(f => f.Create()).Returns(k8sQueryClientMock.Object);
        k8sQueryClientMock.Setup(c => c.GetPodsAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult<IEnumerable<K8sPod>>(
            new K8sPod[] {

            }
        ));
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(true);
        k8sQueryClientMock.Setup(c => c.GetPodAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(podsArray.FirstOrDefault(item => item.Metadata.Name == providerPodName)));

        PodInfoManager target = new PodInfoManager(k8sQueryClientFactoryMock.Object, GetKubeHttpClientSettingsProviderForTest().Object, new IPodNameProvider[] { podNameProviderMock.Object });
        K8sPod result = await target.GetMyPodAsync(default).ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Single(result.Status.ContainerStatuses);
        Assert.Equal("noisy after", result.Status.ContainerStatuses.First().ContainerID); // When pod name is provided directly, do not use container id for matching.
        Assert.Equal(providerPodName, result.Metadata?.Name);
    }

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should fallback to use container id when the name provided by {nameof(IPodNameProvider)} have no hit.")]
    public async Task GetMyPodAsyncShouldFallbackToUseContainerIdWhenProvidedPodNameNotMatch()
    {
        string providerPodName = "podNameByProvider";
        Mock<IK8sQueryClient> k8sQueryClientMock = new();
        Mock<IPodNameProvider> podNameProviderMock = new();
        Mock<IK8sQueryClientFactory> k8sQueryClientFactoryMock = new Mock<IK8sQueryClientFactory>();
        K8sPod[] podsArray = new[]{
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = "noisy" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy in front" } } }},
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = "willgetbycontainerid" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="containerId" } } }},
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = "hello"}, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy after" } } }},
        };

        k8sQueryClientFactoryMock.Setup(f => f.Create()).Returns(k8sQueryClientMock.Object);
        k8sQueryClientMock.Setup(c => c.GetPodsAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult<IEnumerable<K8sPod>>(
            podsArray
        ));
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(true);
        k8sQueryClientMock.Setup(c => c.GetPodAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<K8sPod>(null)); // Always no hit.

        PodInfoManager target = new PodInfoManager(k8sQueryClientFactoryMock.Object, GetKubeHttpClientSettingsProviderForTest().Object, new IPodNameProvider[] { podNameProviderMock.Object });
        K8sPod result = await target.GetMyPodAsync(default).ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Single(result.Status.ContainerStatuses);
        Assert.Equal("containerId", result.Status.ContainerStatuses.First().ContainerID); // When pod name is provided directly, do not use container id for matching.
        Assert.Equal("willgetbycontainerid", result.Metadata?.Name);
    }

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should support multiple {nameof(IPodNameProvider)} instances.")]
    public async Task GetMyPodAsyncShouldSupportMultipleIPodNameProviders()
    {
        string providerPodName = string.Empty;
        string providerPodName2 = "AnotherValidPod";
        K8sPod[] podsArray = new[]{
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = "noisy" }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy in front" } } }},
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = providerPodName2 }, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="matched by pod name provider" } } }},
            new K8sPod(){ Metadata = new K8sPodMetadata { Name = "hello"}, Status = new K8sPodStatus(){ ContainerStatuses= new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="noisy after" } } }},
        };

        Mock<IPodNameProvider> podNameProviderMock = new();
        Mock<IPodNameProvider> podNameProviderMock2 = new();
        Mock<IK8sQueryClient> k8sQueryClientMock = new();
        Mock<IK8sQueryClientFactory> k8sQueryClientFactoryMock = new Mock<IK8sQueryClientFactory>();

        k8sQueryClientFactoryMock.Setup(f => f.Create()).Returns(k8sQueryClientMock.Object);
        k8sQueryClientMock.Setup(c => c.GetPodsAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult<IEnumerable<K8sPod>>(
            podsArray
        ));
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(true);
        k8sQueryClientMock.Setup(c => c.GetPodAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(podsArray.FirstOrDefault(item => item.Metadata.Name == providerPodName2)));
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(false); // the provider returns true with pod name.
        podNameProviderMock2.Setup(p => p.TryGetPodName(out providerPodName2)).Returns(true); // the provider returns true with pod name.

        PodInfoManager target = new PodInfoManager(k8sQueryClientFactoryMock.Object, GetKubeHttpClientSettingsProviderForTest().Object, new IPodNameProvider[] { podNameProviderMock.Object, podNameProviderMock2.Object });
        K8sPod result = await target.GetMyPodAsync(default).ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Single(result.Status.ContainerStatuses);
        Assert.Equal("matched by pod name provider", result.Status.ContainerStatuses.First().ContainerID);
        Assert.Equal(providerPodName2, result.Metadata?.Name);
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
