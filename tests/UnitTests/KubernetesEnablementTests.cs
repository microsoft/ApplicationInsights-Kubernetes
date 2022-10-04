using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

[Collection(FullLoggingCollection.Name)]
public class KubernetesEnablementTest
{
    [Fact(DisplayName = "The required services are properly registered")]
    public void ServicesRegistered()
    {
        Mock<IClusterEnvironmentCheck> clusterCheckMock = new();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(p => new ConfigurationBuilder().Build());

        clusterCheckMock.Setup(c => c.IsInCluster).Returns(true);

        KubernetesServiceCollectionBuilder target = new KubernetesServiceCollectionBuilder(customizeOptions: null, clusterCheckMock.Object);
        target.RegisterServices(services);

        Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

        MockK8sTestEnvironment(services);

        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().First(ti => ti is KubernetesTelemetryInitializer);
            Assert.NotNull(targetTelemetryInitializer);
        }
    }

    private static void MockK8sTestEnvironment(IServiceCollection services)
    {
        // So that it doesn't require a real k8s cluster to get a K8s environment.
        services.AddSingleton<IK8sEnvironmentFactory>(p =>
        {
            Mock<IK8sEnvironmentFactory> k8sEnvFactoryMock = new();
            k8sEnvFactoryMock.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>())).Returns(() =>
            {
                Mock<IK8sEnvironment> k8sEnv = new();
                return Task.FromResult(k8sEnv.Object);
            });
            return k8sEnvFactoryMock.Object;
        });
    }
}
