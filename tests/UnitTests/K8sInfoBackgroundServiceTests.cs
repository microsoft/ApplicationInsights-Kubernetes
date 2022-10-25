using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

public class K8sInfoBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteIsIdempotent()
    {
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        Mock<IServiceScope> serviceScopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        Mock<IK8sEnvironmentFetcher> fetcherMock = new();
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
        serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(p => p.GetService(It.IsAny<Type>())).Returns(fetcherMock.Object);

        AppInsightsForKubernetesOptions options = new AppInsightsForKubernetesOptions();

        IK8sInfoBootstrap target = new K8sInfoBootstrap(serviceScopeFactoryMock.Object, Options.Create(options));

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.5)))
        {
            await target.ExecuteAsync(cancellationTokenSource.Token);
            target.Run();
        }

        fetcherMock.Verify(f => f.UpdateK8sEnvironmentAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteIsIdempotentMultiThreads()
    {
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        Mock<IServiceScope> serviceScopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        Mock<IK8sEnvironmentFetcher> fetcherMock = new();
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
        serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(p => p.GetService(It.IsAny<Type>())).Returns(fetcherMock.Object);

        AppInsightsForKubernetesOptions options = new AppInsightsForKubernetesOptions();

        IK8sInfoBootstrap target = new K8sInfoBootstrap(serviceScopeFactoryMock.Object, Options.Create(options));

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.5)))
        {
            await Parallel.ForEachAsync(Enumerable.Range(1, 100), async (_, _) =>
            {
                await target.ExecuteAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                ((IK8sInfoBootstrap)target).Run();
            });
        }

        fetcherMock.Verify(f => f.UpdateK8sEnvironmentAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
