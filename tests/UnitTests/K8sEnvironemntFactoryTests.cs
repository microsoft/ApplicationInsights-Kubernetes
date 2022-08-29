using System;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class K8sEnvironemntFactoryTests
    {
        [Fact]
        public async Task ShouldTimeoutWaitingPodReady()
        {
            Mock<IContainerIdHolder> containerIdHolderMock = new();
            Mock<IPodInfoManager> podInfoManagerMock = new();
            Mock<IContainerStatusManager> containerStatusManagerMock = new();
            Mock<IK8sClientService> k8sClientServiceMock = new();

            // Timeout
            TimeSpan timeout = TimeSpan.FromMilliseconds(1);

            // Simulate a spin waiting on pod info
            podInfoManagerMock.Setup(p => p.WaitUntilMyPodReadyAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(async (token) =>
                {
                    while (true)
                    {
                        await Task.Delay(timeout);  // This wait is optional, it is there so that the cancellation token check doesn't happen too early.
                        token.ThrowIfCancellationRequested();
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                });

            K8sEnvironmentFactory target = new K8sEnvironmentFactory(containerIdHolderMock.Object, podInfoManagerMock.Object, containerStatusManagerMock.Object, k8sClientServiceMock.Object);

            IK8sEnvironment environment = null;
            CancellationToken timeoutToken;
            using (CancellationTokenSource timeoutSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(1)))
            {
                timeoutToken = timeoutSource.Token;
                environment = await target.CreateAsync(timeoutToken);
            }

            Assert.True(timeoutToken.IsCancellationRequested);
            Assert.Null(environment);
        }

        [Fact]
        public async Task ShouldTimeoutWaitingContainerReady()
        {
            Mock<IContainerIdHolder> containerIdHolderMock = new();
            Mock<IPodInfoManager> podInfoManagerMock = new();
            Mock<IContainerStatusManager> containerStatusManagerMock = new();
            Mock<IK8sClientService> k8sClientServiceMock = new();

            // Timeout
            TimeSpan timeout = TimeSpan.FromMilliseconds(1);

            // Pod will be returned.
            V1Pod pod = new V1Pod();
            podInfoManagerMock.Setup(p => p.WaitUntilMyPodReadyAsync(It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(pod));

            // Simulate a delay on getting container ready
            containerStatusManagerMock.Setup(c => c.WaitContainerReadyAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(async (token) =>
            {
                while (true)
                {
                    await Task.Delay(timeout);  // This wait is optional, it is there so that the cancellation token check doesn't happen too early.
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });

            K8sEnvironmentFactory target = new K8sEnvironmentFactory(containerIdHolderMock.Object, podInfoManagerMock.Object, containerStatusManagerMock.Object, k8sClientServiceMock.Object);

            IK8sEnvironment environment = null;
            CancellationToken timeoutToken;
            using (CancellationTokenSource timeoutSource = new CancellationTokenSource(timeout))
            {
                timeoutToken = timeoutSource.Token;
                environment = await target.CreateAsync(timeoutToken);
            }

            Assert.True(timeoutToken.IsCancellationRequested);
            Assert.Null(environment);
        }
    }
}
