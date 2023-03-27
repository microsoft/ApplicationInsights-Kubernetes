using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.ApplicationInsights.Kubernetes.Pods;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

public class ContainerStatusManagerTests
{
    /// <summary>
    /// Regression coverage: when there's no container id provided, falls back to use any container status in the pod.
    /// </summary>
    [Fact]
    public async Task GetReadyWhenNoContainerId()
    {
        Mock<IPodInfoManager> podInfoManagerMock = new();
        Mock<IContainerIdHolder> containerIdHolderMock = new();

        // At least 1 container is ready.
        V1Pod testPod = new V1Pod()
        {
            Status = new V1PodStatus()
            {
                ContainerStatuses = new List<V1ContainerStatus>(){
                    new V1ContainerStatus(){
                        Ready = true,
                    }
                }
            }
        };
        podInfoManagerMock.Setup(p => p.GetMyPodAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(testPod));

        ContainerStatusManager containerStatusManager = new ContainerStatusManager(podInfoManagerMock.Object, containerIdHolderMock.Object);

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
        {
            // My container status is absent since the container id is not available.
            V1ContainerStatus myContainerStatus = await containerStatusManager.GetMyContainerStatusAsync(cancellationTokenSource.Token);
            Assert.Null(myContainerStatus);

            // Since there's a container ready:
            bool isReady = await containerStatusManager.IsContainerReadyAsync(cancellationTokenSource.Token);
            Assert.True(isReady);
        }
    }
}
