using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            services = services.AddApplicationInsightsKubernetesEnricher(applyOptions: null, kubernetesServiceCollectionBuilder: null, detectKubernetes: () => true, logger: null);
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

            // Replace the IKubeHttpClientSettingsProvider in case the test is not running inside a container.
            Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IKubeHttpClientSettingsProvider)));
            Mock<IKubeHttpClientSettingsProvider> mock = new Mock<IKubeHttpClientSettingsProvider>();
            services.Remove(new ServiceDescriptor(typeof(IKubeHttpClientSettingsProvider), typeof(KubeHttpClientSettingsProvider), ServiceLifetime.Singleton));
            services.AddSingleton<IKubeHttpClientSettingsProvider>(p => mock.Object);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            // Logging
            serviceProvider.GetRequiredService<ILoggerFactory>();
            serviceProvider.GetRequiredService<ILogger<KubernetesEnablementTest>>();

            // K8s services
            serviceProvider.GetRequiredService<IKubeHttpClientSettingsProvider>();
            serviceProvider.GetRequiredService<KubeHttpClientFactory>();
            serviceProvider.GetRequiredService<K8sQueryClientFactory>();
            serviceProvider.GetRequiredService<IK8sEnvironmentFactory>();
        }

        [Fact(DisplayName = "Default timeout for waiting container to spin us is 2 minutes")]
        public void EnableAppInsightsForKubernetesWithDefaultTimeOut()
        {
            IServiceCollection services = new ServiceCollection();
            services = services.AddApplicationInsightsKubernetesEnricher(
                applyOptions:null,
                kubernetesServiceCollectionBuilder: null,
                detectKubernetes: () => true,
                logger: null);
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

            // Replace the IKubeHttpClientSettingsProvider in case the test is not running inside a container.
            Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IKubeHttpClientSettingsProvider)));
            Mock<IKubeHttpClientSettingsProvider> mock = new Mock<IKubeHttpClientSettingsProvider>();
            services.Remove(new ServiceDescriptor(typeof(IKubeHttpClientSettingsProvider), typeof(KubeHttpClientSettingsProvider), ServiceLifetime.Singleton));
            services.AddSingleton<IKubeHttpClientSettingsProvider>(p => mock.Object);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

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
            IServiceCollection services = new ServiceCollection();
            services = services.AddApplicationInsightsKubernetesEnricher(applyOptions:
                option =>
                {
                    option.InitializationTimeout = TimeSpan.FromSeconds(5);
                }, kubernetesServiceCollectionBuilder: null, detectKubernetes: () => true, logger: null);
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

            // Replace the IKubeHttpClientSettingsProvider in case the test is not running inside a container.
            Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IKubeHttpClientSettingsProvider)));
            Mock<IKubeHttpClientSettingsProvider> mock = new Mock<IKubeHttpClientSettingsProvider>();
            services.Remove(new ServiceDescriptor(typeof(IKubeHttpClientSettingsProvider), typeof(KubeHttpClientSettingsProvider), ServiceLifetime.Singleton));
            services.AddSingleton<IKubeHttpClientSettingsProvider>(p => mock.Object);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            if (targetTelemetryInitializer is KubernetesTelemetryInitializer target)
            {
                Assert.StrictEqual(TimeSpan.FromSeconds(5), target._options.InitializationTimeout);
            }
            else
            {
                Assert.True(false, "Not the target telemetry initializer.");
            }
        }

        [Fact(DisplayName = "Set timeout through configuration works for telemetry initializer.")]
        public void EnableAppInsightsForKubernetesWithTimeOutSetThroughConfiguration()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>(){
                    {"a" , "b"},
                    {"AppInsightsForKubernetes:InitializationTimeout", "3.1:12:15.34"}
            }).Build();
            services.AddSingleton<IConfiguration>(config);

            services = services.AddApplicationInsightsKubernetesEnricher(
                applyOptions: null, kubernetesServiceCollectionBuilder: null, detectKubernetes: () => true, logger: null);
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

            // Replace the IKubeHttpClientSettingsProvider in case the test is not running inside a container.
            Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IKubeHttpClientSettingsProvider)));
            Mock<IKubeHttpClientSettingsProvider> mock = new Mock<IKubeHttpClientSettingsProvider>();
            services.Remove(new ServiceDescriptor(typeof(IKubeHttpClientSettingsProvider), typeof(KubeHttpClientSettingsProvider), ServiceLifetime.Singleton));
            services.AddSingleton<IKubeHttpClientSettingsProvider>(p => mock.Object);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            if (targetTelemetryInitializer is KubernetesTelemetryInitializer target)
            {
                Assert.StrictEqual(new TimeSpan(days: 3, hours: 1, minutes: 12, seconds: 15, milliseconds: 340), target._options.InitializationTimeout);
            }
            else
            {
                Assert.True(false, "Not the target telemetry initializer.");
            }
        }

        [Fact(DisplayName = "Set timeout through configuration works for telemetry initializer.")]
        public void EnableAppInsightsForKubernetesWithTimeOutSetThroughOptionsOverwritingConfiugure()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>(){
                    {"a" , "b"},
                    {"AppInsightsForKubernetes:InitializationTimeout", "3.1:12:15.34"}
            }).Build();
            services.AddSingleton<IConfiguration>(config);

            services = services.AddApplicationInsightsKubernetesEnricher(
                applyOptions: option =>
                {
                    option.InitializationTimeout = TimeSpan.FromSeconds(30);
                }, kubernetesServiceCollectionBuilder: null, detectKubernetes: () => true, logger: null);
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

            // Replace the IKubeHttpClientSettingsProvider in case the test is not running inside a container.
            Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IKubeHttpClientSettingsProvider)));
            Mock<IKubeHttpClientSettingsProvider> mock = new Mock<IKubeHttpClientSettingsProvider>();
            services.Remove(new ServiceDescriptor(typeof(IKubeHttpClientSettingsProvider), typeof(KubeHttpClientSettingsProvider), ServiceLifetime.Singleton));
            services.AddSingleton<IKubeHttpClientSettingsProvider>(p => mock.Object);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);

            if (targetTelemetryInitializer is KubernetesTelemetryInitializer target)
            {
                Assert.StrictEqual(TimeSpan.FromSeconds(30), target._options.InitializationTimeout);
            }
            else
            {
                Assert.True(false, "Not the target telemetry initializer.");
            }
        }

        [Fact(DisplayName = "Support adding KubernetesTelemetryInitializer to given TelemetryConfiguration")]
        public void AddTheInitializerToGivenConfiguration()
        {
            Mock<ITelemetryChannel> channelMock = new Mock<ITelemetryChannel>();
            Mock<IKubernetesServiceCollectionBuilder> serviceCollectionBuilderMock = new Mock<IKubernetesServiceCollectionBuilder>();
            ServiceCollection sc = new ServiceCollection();
            sc.AddLogging();
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
                    SDKVersionUtils.Instance,
                    p.GetService<ILogger<KubernetesTelemetryInitializer>>());
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
