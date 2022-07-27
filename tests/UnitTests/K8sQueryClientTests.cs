using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;
public class K8sQueryClientTests
{
    [Fact(DisplayName = "Constructor should throw given null KubeHttpClient")]
    public void CtorNullHttpClientThrows()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
        {
            new K8sQueryClient(null);
        });

        Assert.Equal("k8sClientService", ex.ParamName);
    }

    [Fact(DisplayName = $"Constructor should set {nameof(IK8sClientService)}")]
    public void CtorSetsKubeHttpClient()
    {
        Mock<IK8sClientService> k8sClientServiceMock = new();
        K8sQueryClient target = new K8sQueryClient(k8sClientServiceMock.Object);

        Assert.Same(k8sClientServiceMock.Object, target.K8sClientService);
    }

    [Fact]
    public async Task ShouldGetAllPods()
    {
        const string testNamespace = nameof(testNamespace);
        Mock<IK8sClientService> k8sClientService = new();
        K8sQueryClient target = new K8sQueryClient(k8sClientService.Object);
        IEnumerable<V1Pod> podListResult = new V1Pod[]{
            new V1Pod(){ Metadata = new V1ObjectMeta(){ Name="pod1" }, Status = new V1PodStatus(){ ContainerStatuses = new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="c1" } } }},
            new V1Pod(){ Metadata = new V1ObjectMeta(){ Name="pod2" }, Status = new V1PodStatus(){ ContainerStatuses = new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="c2" } } }},
        };

        k8sClientService.Setup(c => c.ListPodsAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(podListResult));
        IEnumerable<V1Pod> results = await target.GetPodsAsync(default);

        Assert.NotNull(results);
        Assert.Equal(podListResult, results);
    }

    [Fact]
    public async Task ShouldGetASinglePodOrNull()
    {
        const string testNamespace = nameof(testNamespace);
        Mock<IK8sClientService> k8sClientService = new();
        K8sQueryClient target = new K8sQueryClient(k8sClientService.Object);
        IEnumerable<V1Pod> podListResult = new V1Pod[]{
            new V1Pod(){ Metadata = new V1ObjectMeta(){ Name="pod1" }, Status = new V1PodStatus(){ ContainerStatuses = new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="c1" } } }},
            new V1Pod(){ Metadata = new V1ObjectMeta(){ Name="pod2" }, Status = new V1PodStatus(){ ContainerStatuses = new List<V1ContainerStatus>(){ new V1ContainerStatus() { ContainerID="c2" } } }},
        };

        k8sClientService.Setup(c => c.GetPodByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns<string, CancellationToken>((p, c) =>
        {
            return Task.FromResult(podListResult.FirstOrDefault(r => string.Equals(r.Metadata.Name, p)));
        });

        // Get pod by name
        V1Pod targetPod = await target.GetPodAsync("pod2", default);
        Assert.NotNull(targetPod);
        Assert.Equal("pod2", targetPod.Metadata.Name);

        // Get null when there's no hit
        targetPod = await target.GetPodAsync("pod3", default);
        Assert.Null(targetPod);
    }

    [Fact]
    public async Task ShouldGetReplicasSets()
    {
        const string testNamespace = nameof(testNamespace);
        Mock<IK8sClientService> k8sClientService = new();
        K8sQueryClient target = new K8sQueryClient(k8sClientService.Object);
        IEnumerable<V1ReplicaSet> replicaSets = new V1ReplicaSet[]{
            new V1ReplicaSet(){ Metadata = new V1ObjectMeta(){ Name="replicaSet1" }},
            new V1ReplicaSet(){ Metadata = new V1ObjectMeta(){ Name="replicaSet2" }},
        };

        k8sClientService.Setup(c => c.ListReplicaSetsAsync(It.IsAny<CancellationToken>())).Returns(() =>
        {
            return Task.FromResult(replicaSets);
        });

        IEnumerable<V1ReplicaSet> actual = await target.GetReplicasAsync(cancellationToken: default);

        Assert.NotNull(actual);
        Assert.Equal(replicaSets, actual);
    }

    [Fact(DisplayName = "GetDeploymentsAsync should request the proper uri")]
    public async Task ShouldGetDeployments()
    {
        const string testNamespace = nameof(testNamespace);
        Mock<IK8sClientService> k8sClientService = new();
        K8sQueryClient target = new K8sQueryClient(k8sClientService.Object);
        IEnumerable<V1Deployment> deployments = new V1Deployment[]{
            new V1Deployment(){ Metadata = new V1ObjectMeta(){ Name="deployment1" }},
            new V1Deployment(){ Metadata = new V1ObjectMeta(){ Name="deployment2" }},
        };

        k8sClientService.Setup(c => c.ListDeploymentsAsync(It.IsAny<CancellationToken>())).Returns(() =>
        {
            return Task.FromResult(deployments);
        });

        IEnumerable<V1Deployment> actual = await target.GetDeploymentsAsync(cancellationToken: default);

        Assert.NotNull(actual);
        Assert.Equal(deployments, actual);
    }

    [Fact]
    public async Task ShouldGetNodes()
    {
        const string testNamespace = nameof(testNamespace);
        Mock<IK8sClientService> k8sClientService = new();
        K8sQueryClient target = new K8sQueryClient(k8sClientService.Object);
        IEnumerable<V1Node> nodes = new V1Node[]{
            new V1Node(){ Metadata = new V1ObjectMeta(){ Name="node1" }},
            new V1Node(){ Metadata = new V1ObjectMeta(){ Name="node2" }},
        };

        k8sClientService.Setup(c => c.ListNodesAsync(It.IsAny<CancellationToken>())).Returns(() =>
        {
            return Task.FromResult(nodes);
        });

        IEnumerable<V1Node> actual = await target.GetNodesAsync(cancellationToken: default);

        Assert.NotNull(actual);
        Assert.Equal(nodes, actual);
    }
}
