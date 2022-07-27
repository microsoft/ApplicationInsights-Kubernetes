using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;
public class KubernetesEnablementTest
{
    [Fact(DisplayName = "The required services are properly registered")]
    public void ServicesRegistered()
    {
        Mock<IClusterCheck> clusterCheckMock = new();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(p => new ConfigurationBuilder().Build());

        clusterCheckMock.Setup(c => c.IsInCluster()).Returns(true);

        KubernetesServiceCollectionBuilder target = new KubernetesServiceCollectionBuilder(customizeOptions: null, clusterCheckMock.Object);
        target.RegisterServices(services);

        Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));
    }
}
