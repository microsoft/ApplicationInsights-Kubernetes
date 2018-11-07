using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    public class KubernetesEnablementTest
    {
        [Fact(DisplayName = "The required services are properly injected")]
        public void ServiceInjected()
        {
            IServiceCollection services = new ServiceCollection();
            services = services.AddAppInsightsTelemetryKubernetesEnricher(detectKubernetes: () => true);

            // Replace the IKubeHttpClientSetingsProvider in case the test is not running inside a container.
            Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IKubeHttpClientSettingsProvider)));
            Mock<IKubeHttpClientSettingsProvider> mock = new Mock<IKubeHttpClientSettingsProvider>();
            services.Remove(new ServiceDescriptor(typeof(IKubeHttpClientSettingsProvider), typeof(KubeHttpClientSettingsProvider), ServiceLifetime.Singleton));
            services.AddSingleton<IKubeHttpClientSettingsProvider>(p => mock.Object);

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Logging
            serviceProvider.GetRequiredService<ILoggerFactory>();
            serviceProvider.GetRequiredService<ILogger<KubernetesEnablementTest>>();

            // K8s services
            serviceProvider.GetRequiredService<IKubeHttpClientSettingsProvider>();
            serviceProvider.GetRequiredService<KubeHttpClientFactory>();
            serviceProvider.GetRequiredService<K8sQueryClientFactory>();
            serviceProvider.GetRequiredService<IK8sEnvironmentFactory>();
        }

        [Fact(DisplayName = "Support adding KubernetesTelemetryInitializer to given TelemetryConfiguration")]
        public void AddTheInitializerToGivenConfiguration()
        {
            Mock<ITelemetryChannel> channelMock = new Mock<ITelemetryChannel>();
            Mock<IKubernetesServiceCollectionBuilder> serviceCollectionBuilderMock = new Mock<IKubernetesServiceCollectionBuilder>();
            ServiceCollection sc = new ServiceCollection();
            sc.AddLogging();

            sc.AddSingleton<ITelemetryInitializer>(p =>
            {
                var envMock = new Mock<IK8sEnvironment>();
                envMock.Setup(env => env.ContainerID).Returns("Cid");
                var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
                envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(envMock.Object, TimeSpan.FromMinutes(1));
                return new KubernetesTelemetryInitializer(envFactoryMock.Object, TimeSpan.FromSeconds(5), SDKVersionUtils.Instance, p.GetService<ILogger<KubernetesTelemetryInitializer>>());
            });
            serviceCollectionBuilderMock.Setup(b => b.InjectServices(It.IsAny<IServiceCollection>(), It.IsAny<TimeSpan>()))
                .Returns(sc);

            TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration("123", channelMock.Object);
            telemetryConfiguration.AddAppInsightsTelemetryKubernetesEnricher(null, serviceCollectionBuilderMock.Object, detectKubernetes: () => true);

            Assert.NotNull(telemetryConfiguration.TelemetryInitializers);
            Assert.True(telemetryConfiguration.TelemetryInitializers.Count == 1);
            Assert.True(telemetryConfiguration.TelemetryInitializers.First() is KubernetesTelemetryInitializer);
        }
    }
}
