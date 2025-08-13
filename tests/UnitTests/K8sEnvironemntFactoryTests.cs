using System;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.ApplicationInsights.Kubernetes.Pods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class K8sEnvironemntFactoryTests
    {
        [Fact]
        public async Task ShouldTimeoutWaitingPodReady()
        {
            Mock<IPodInfoManager> podInfoManagerMock = new();
            Mock<IContainerStatusManager> containerStatusManagerMock = new();
            Mock<IK8sClientService> k8sClientServiceMock = new();
            Mock<IOptions<AppInsightsForKubernetesOptions>> appInsightsForKubernetesOptionsMock = new();
            appInsightsForKubernetesOptionsMock
                .Setup(o => o.Value)
                .Returns(new AppInsightsForKubernetesOptions());

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

            K8sEnvironmentFactory target = new K8sEnvironmentFactory(podInfoManagerMock.Object,
                containerStatusManagerMock.Object, k8sClientServiceMock.Object,
                appInsightsForKubernetesOptionsMock.Object);
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

        [Fact(Skip = "The scenario covered is deprecated.")]
        [Obsolete("The scenario covered is deprecated", error: false)]
        public async Task ShouldTimeoutWaitingContainerReady()
        {
            Mock<IPodInfoManager> podInfoManagerMock = new();
            Mock<IContainerStatusManager> containerStatusManagerMock = new();
            Mock<IK8sClientService> k8sClientServiceMock = new();
            Mock<IOptions<AppInsightsForKubernetesOptions>> appInsightsForKubernetesOptionsMock = new();
            appInsightsForKubernetesOptionsMock
                .Setup(o => o.Value)
                .Returns(new AppInsightsForKubernetesOptions());

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

            K8sEnvironmentFactory target = new K8sEnvironmentFactory(podInfoManagerMock.Object,
                containerStatusManagerMock.Object, k8sClientServiceMock.Object,
                appInsightsForKubernetesOptionsMock.Object);

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
