using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;

public class PodInfoManagerTests
{
    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should get my pod correctly")]
    public async Task GetMyPodAsyncShouldGetCorrectPod()
    {
        Mock<IK8sClientService> k8sQueryClientMock = new();
        Mock<IContainerIdHolder> containerIdHolderMock = new();
        Mock<IPodNameProvider> podNameProviderMock = new();

        k8sQueryClientMock.Setup(c => c.GetPodsAsync(It.IsAny<CancellationToken>())).Returns(() =>
        {
            return Task.FromResult<IEnumerable<V1Pod>>(new V1Pod[]{
                    new V1Pod(status: new V1PodStatus(){ ContainerStatuses = new List<V1ContainerStatus>{ new V1ContainerStatus() { ContainerID = "noisy in front" }}}),
                    new V1Pod(status: new V1PodStatus(){ ContainerStatuses = new List<V1ContainerStatus>{ new V1ContainerStatus() { ContainerID = "containerId" }}}),
                    new V1Pod(status: new V1PodStatus(){ ContainerStatuses = new List<V1ContainerStatus>{ new V1ContainerStatus() { ContainerID = "noisy after" }}}),
            });
        });

        containerIdHolderMock.Setup(c => c.ContainerId).Returns("containerId");

        PodInfoManager target = new PodInfoManager(k8sQueryClientMock.Object, containerIdHolderMock.Object, new IPodNameProvider[] { podNameProviderMock.Object });
        V1Pod result = await target.GetMyPodAsync(default).ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Single(result.Status.ContainerStatuses);
        Assert.Equal("containerId", result.Status.ContainerStatuses.First().ContainerID);
    }

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should use pod name provided by {nameof(IPodNameProvider)} first when possible")]
    public async Task GetMyPodAsyncShouldLeveragePodNameProviders()
    {
        string providerPodName = "podNameByProvider";

        Mock<IK8sClientService> k8sQueryClientMock = new();
        Mock<IContainerIdHolder> containerIdHolderMock = new();
        Mock<IPodNameProvider> podNameProviderMock = new();

        V1Pod[] podsArray = new[]{
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = "noisy" }, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="noisy in front" } } }},
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = "another noise" }, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="containerId" } } }},
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = providerPodName}, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="noisy after" } } }},
        };

        k8sQueryClientMock.Setup(c => c.GetPodsAsync(It.IsAny<CancellationToken>())).Returns(() => Task.FromResult<IEnumerable<V1Pod>>(podsArray));
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(true);
        k8sQueryClientMock.Setup(c => c.GetPodByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(podsArray.FirstOrDefault(item => item.Metadata.Name == providerPodName)));

        PodInfoManager target = new PodInfoManager(k8sQueryClientMock.Object, containerIdHolderMock.Object, new IPodNameProvider[] { podNameProviderMock.Object });
        V1Pod result = await target.GetMyPodAsync(default).ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Single(result.Status.ContainerStatuses);
        Assert.Equal("noisy after", result.Status.ContainerStatuses.First().ContainerID); // When pod name is provided directly, do not use container id for matching.
        Assert.Equal(providerPodName, result.Metadata?.Name);
    }

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should fallback to use container id when the name provided by {nameof(IPodNameProvider)} have no hit.")]
    public async Task GetMyPodAsyncShouldFallbackToUseContainerIdWhenProvidedPodNameNotMatch()
    {
        string providerPodName = "podNameByProvider";
        const string targetContainerId = "containerId";

        Mock<IK8sClientService> k8sQueryClientMock = new();
        Mock<IContainerIdHolder> containerIdHolderMock = new();
        Mock<IPodNameProvider> podNameProviderMock = new();

        V1Pod[] podsArray = new[]{
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = "noisy" }, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="noisy in front" } } }},
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = "willgetbycontainerid" }, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID=targetContainerId } } }},
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = "hello"}, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="noisy after" } } }},
        };

        k8sQueryClientMock.Setup(c => c.GetPodsAsync(It.IsAny<CancellationToken>())).Returns(() => Task.FromResult<IEnumerable<V1Pod>>(podsArray));
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(true);
        k8sQueryClientMock.Setup(c => c.GetPodByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<V1Pod>(null)); // Always no hit.
        containerIdHolderMock.Setup(c => c.ContainerId).Returns(targetContainerId);

        PodInfoManager target = new PodInfoManager(k8sQueryClientMock.Object, containerIdHolderMock.Object, new IPodNameProvider[] { podNameProviderMock.Object });
        V1Pod result = await target.GetMyPodAsync(default).ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Single(result.Status.ContainerStatuses);
        Assert.Equal(targetContainerId, result.Status.ContainerStatuses.First().ContainerID); // When pod name is provided directly, do not use container id for matching.
        Assert.Equal("willgetbycontainerid", result.Metadata?.Name);
    }

    [Fact(DisplayName = $"{nameof(PodInfoManager.GetMyPodAsync)} should support multiple {nameof(IPodNameProvider)} instances.")]
    public async Task GetMyPodAsyncShouldSupportMultipleIPodNameProviders()
    {
        string providerPodName = string.Empty;
        string providerPodName2 = "AnotherValidPod";
        V1Pod[] podsArray = new[]{
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = "noisy" }, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="noisy in front" } } }},
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = providerPodName2 }, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="matched by pod name provider" } } }},
            new V1Pod(){ Metadata = new V1ObjectMeta { Name = "hello"}, Status = new V1PodStatus(){ ContainerStatuses= new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="noisy after" } } }},
        };

        Mock<IPodNameProvider> podNameProviderMock = new();
        Mock<IPodNameProvider> podNameProviderMock2 = new();
        Mock<IK8sClientService> k8sQueryClientMock = new();
        Mock<IContainerIdHolder> containerIdHolderMock = new();

        k8sQueryClientMock.Setup(c => c.GetPodsAsync(It.IsAny<CancellationToken>())).Returns(() => Task.FromResult<IEnumerable<V1Pod>>(podsArray));
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(true);
        k8sQueryClientMock.Setup(c => c.GetPodByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(podsArray.FirstOrDefault(item => item.Metadata.Name == providerPodName2)));
        podNameProviderMock.Setup(p => p.TryGetPodName(out providerPodName)).Returns(false); // the provider returns true with pod name.
        podNameProviderMock2.Setup(p => p.TryGetPodName(out providerPodName2)).Returns(true); // the provider returns true with pod name.

        PodInfoManager target = new PodInfoManager(k8sQueryClientMock.Object, containerIdHolderMock.Object, new IPodNameProvider[] { podNameProviderMock.Object, podNameProviderMock2.Object });
        V1Pod result = await target.GetMyPodAsync(default).ConfigureAwait(false);

        Assert.NotNull(result);
        Assert.Single(result.Status.ContainerStatuses);
        Assert.Equal("matched by pod name provider", result.Status.ContainerStatuses.First().ContainerID);
        Assert.Equal(providerPodName2, result.Metadata?.Name);
    }

    [Fact]
    public void TryGetContainerStatusShallReturnTheStatusOnMatch()
    {
        Mock<IK8sClientService> k8sQueryClientMock = new();
        Mock<IContainerIdHolder> containerIdHolderMock = new();

        string containerId = "b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4";
        string containerName = "testContainerName";
        V1Pod pod = new V1Pod();
        pod.Status = new V1PodStatus()
        {
            ContainerStatuses = new V1ContainerStatus[]{
                new V1ContainerStatus(){
                    Name=containerName,
                    ContainerID = "docker://b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4"
                },
            },
        };

        PodInfoManager target = new PodInfoManager(k8sQueryClientMock.Object, containerIdHolderMock.Object, new IPodNameProvider[] { });

        bool result = target.TryGetContainerStatus(pod, containerId, out V1ContainerStatus containerStatus);
        Assert.True(result);
        Assert.NotNull(containerStatus);
        Assert.Equal(containerName, containerStatus.Name);
    }

    [Fact]
    public void TryGetContainerStatusShallReturnNullWhenContainerIdNullOrEmpty()
    {
        Mock<IK8sClientService> k8sQueryClientMock = new();
        Mock<IContainerIdHolder> containerIdHolderMock = new();

        V1Pod pod = new V1Pod();
        pod.Status = new V1PodStatus()
        {
            ContainerStatuses = new V1ContainerStatus[]{
                new V1ContainerStatus(){
                    Name="testContainerName",
                    ContainerID = "docker://b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4"
                },
            },
        };

        PodInfoManager target = new PodInfoManager(k8sQueryClientMock.Object, containerIdHolderMock.Object, new IPodNameProvider[] { });

        // Container id is null
        bool result = target.TryGetContainerStatus(pod, containerId: null, out V1ContainerStatus nullContainerIdContainerStatus);
        Assert.False(result);
        Assert.Null(nullContainerIdContainerStatus);

        // Container id is String.Empty
        bool result2 = target.TryGetContainerStatus(pod, containerId: string.Empty, out V1ContainerStatus emptyStringContainerIdStatus);
        Assert.False(result);
        Assert.Null(emptyStringContainerIdStatus);

        // Container id is ""
        bool result3 = target.TryGetContainerStatus(pod, containerId: "", out V1ContainerStatus literalEmptyStringContainerIdStatus);
        Assert.False(result3);
        Assert.Null(literalEmptyStringContainerIdStatus);
    }
}
