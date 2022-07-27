using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
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

        MockK8sTestEnvironment(services);

        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().First(ti => ti is KubernetesTelemetryInitializer);
            Assert.NotNull(targetTelemetryInitializer);
        }
    }

    [Fact(DisplayName = "Default timeout for waiting container to spin us is 2 minutes")]
    public void EnableAppInsightsForKubernetesWithDefaultTimeOut()
    {
        Mock<IClusterCheck> clusterCheckMock = new();
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(p => new ConfigurationBuilder().Build());

        clusterCheckMock.Setup(c => c.IsInCluster()).Returns(true);

        KubernetesServiceCollectionBuilder target = new KubernetesServiceCollectionBuilder(customizeOptions: null, clusterCheckMock.Object);
        target.RegisterServices(services);

        MockK8sTestEnvironment(services);

        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            KubernetesTelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().First(ti => ti is KubernetesTelemetryInitializer) as KubernetesTelemetryInitializer;
            Assert.StrictEqual(TimeSpan.FromMinutes(2), targetTelemetryInitializer.Options.InitializationTimeout);
        }
    }

    [Fact(DisplayName = "Set timeout through options works for telemetry initializer.")]
    public void EnableAppInsightsForKubernetesWithTimeOutSetThroughOptions()
    {
        Mock<IClusterCheck> clusterCheckMock = new();
        clusterCheckMock.Setup(c => c.IsInCluster()).Returns(true);

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(p => new ConfigurationBuilder().Build());
        services.ConfigureKubernetesTelemetryInitializer(
            overwriteOptions: option =>
            {
                option.InitializationTimeout = TimeSpan.FromSeconds(5);
            }, clusterCheckMock.Object);

        MockK8sTestEnvironment(services);

        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().First(i => i is KubernetesTelemetryInitializer);
            if (targetTelemetryInitializer is KubernetesTelemetryInitializer target)
            {
                Assert.StrictEqual(TimeSpan.FromSeconds(5), target.Options.InitializationTimeout);
            }
            else
            {
                Assert.True(false, "Not the target telemetry initializer.");
            }
        }
    }

    [Fact(DisplayName = "Set timeout through configuration works for telemetry initializer.")]
    public void EnableAppInsightsForKubernetesWithTimeOutSetThroughConfiguration()
    {
        Mock<IClusterCheck> clusterCheckMock = new();
        clusterCheckMock.Setup(c => c.IsInCluster()).Returns(true);

        IServiceCollection services = new ServiceCollection();
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string>(){
                    {"a" , "b"},
                    {"AppInsightsForKubernetes:InitializationTimeout", "3.1:12:15.34"}
        }).Build();
        services.AddSingleton(config);
        KubernetesServiceCollectionBuilder k8sServiceBuilder = new KubernetesServiceCollectionBuilder(customizeOptions: null, clusterCheckMock.Object);
        k8sServiceBuilder.RegisterServices(services);

        MockK8sTestEnvironment(services);

        Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            if (targetTelemetryInitializer is KubernetesTelemetryInitializer target)
            {
                Assert.StrictEqual(new TimeSpan(days: 3, hours: 1, minutes: 12, seconds: 15, milliseconds: 340), target.Options.InitializationTimeout);
            }
            else
            {
                Assert.True(false, "Not the target telemetry initializer.");
            }
        }
    }

    private static void MockK8sTestEnvironment(IServiceCollection services)
    {
        // So that it doesn't require a real k8s cluster to get a K8s environment.
        services.AddSingleton<IK8sEnvironmentFactory>(p =>
        {
            Mock<IK8sEnvironmentFactory> k8sEnvFactoryMock = new();
            k8sEnvFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(() =>
            {
                Mock<IK8sEnvironment> k8sEnv = new();
                return Task.FromResult(k8sEnv.Object);
            });
            return k8sEnvFactoryMock.Object;
        });
    }
}
