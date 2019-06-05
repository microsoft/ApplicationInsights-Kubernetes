using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    public class KubernetesEnablementTest
    {
        [Fact(DisplayName = "Default timeout for waiting container to spin us is 2 minutes")]
        public void EnableAppInsightsForKubernetesWithDefaultTimeOut()
        {
            TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration();
            telemetryConfiguration.AddApplicationInsightsKubernetesEnricher(
                    applyOptions: null,
                    kubernetesServiceCollectionBuilder: new KubernetesTestServiceCollectionBuilder(),
                    detectKubernetes: () => true);
            ITelemetryInitializer targetTelemetryInitializer = telemetryConfiguration.TelemetryInitializers.FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            if (targetTelemetryInitializer is KubernetesTelemetryInitializer target)
            {
                Assert.StrictEqual(TimeSpan.FromMinutes(2), target._options.InitializationTimeout);
            }
            else
            {
                Assert.True(false, "Not the target telemetry initializer.");
            }
        }

        [Fact(DisplayName = "Set timeout through options works for telemetry initializer.")]
        public void EnableAppInsightsForKubernetesWithTimeOutSetThroughOptions()
        {
            TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration();
            telemetryConfiguration.AddApplicationInsightsKubernetesEnricher(
                    applyOptions: option =>
                    {
                        option.InitializationTimeout = TimeSpan.FromSeconds(5);
                    },
                    kubernetesServiceCollectionBuilder: new KubernetesTestServiceCollectionBuilder(),
                    detectKubernetes: () => true);
            ITelemetryInitializer targetTelemetryInitializer = telemetryConfiguration.TelemetryInitializers.FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            if (targetTelemetryInitializer is KubernetesTelemetryInitializer target)
            {
                Assert.StrictEqual(TimeSpan.FromSeconds(5), target._options.InitializationTimeout);
            }
            else
            {
                Assert.True(false, "Not the target telemetry initializer.");
            }
        }

        [Fact(DisplayName = "Set timeout through configuration works for telemetry initializer.", Skip = "Need to find a way to make this testable.")]
        public void EnableAppInsightsForKubernetesWithTimeOutSetThroughConfiguration()
        {
        }

        [Fact(DisplayName = "Set timeout through configuration, but then overwritten by options, for telemetry initializer.",
            Skip = "Need to find a way to make this testable.")]
        public void EnableAppInsightsForKubernetesWithTimeOutSetThroughOptionsOverwritingConfiugure()
        {
        }

        [Fact(DisplayName = "Support adding KubernetesTelemetryInitializer to given TelemetryConfiguration")]
        public void AddTheInitializerToGivenConfiguration()
        {
            Mock<ITelemetryChannel> channelMock = new Mock<ITelemetryChannel>();
            Mock<IKubernetesServiceCollectionBuilder> serviceCollectionBuilderMock = new Mock<IKubernetesServiceCollectionBuilder>();
            ServiceCollection sc = new ServiceCollection();
            sc.AddOptions();
            sc.Configure<AppInsightsForKubernetesOptions>(options => options.InitializationTimeout = TimeSpan.FromSeconds(5));

            sc.AddSingleton<ITelemetryInitializer>(p =>
            {
                var envMock = new Mock<IK8sEnvironment>();
                envMock.Setup(env => env.ContainerID).Returns("Cid");
                var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
                envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(envMock.Object, TimeSpan.FromMinutes(1));
                return new KubernetesTelemetryInitializer(
                    envFactoryMock.Object,
                    p.GetService<IOptions<AppInsightsForKubernetesOptions>>(),
                    SDKVersionUtils.Instance);
            });
            serviceCollectionBuilderMock.Setup(b => b.InjectServices(It.IsAny<IServiceCollection>()))
                .Returns(sc);

            TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration("123", channelMock.Object);
            telemetryConfiguration.AddApplicationInsightsKubernetesEnricher(
                applyOptions: null,
                kubernetesServiceCollectionBuilder: serviceCollectionBuilderMock.Object,
                detectKubernetes: () => true);

            Assert.NotNull(telemetryConfiguration.TelemetryInitializers);
            Assert.Single(telemetryConfiguration.TelemetryInitializers);
            Assert.True(telemetryConfiguration.TelemetryInitializers.First() is KubernetesTelemetryInitializer);
        }
    }
}
