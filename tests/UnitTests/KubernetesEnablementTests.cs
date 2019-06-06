using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class KubernetesEnablementTest
    {
        [Fact(DisplayName = "The required services are properly injected")]
        public void ServiceInjected()
        {
            IServiceCollection services = new ServiceCollection();
            ApplicationInsightsExtensions.InjectKubernetesTelemetryInitializer(services, () => true, new KubernetesTestServiceCollectionBuilder(), null);
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));
            
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().FirstOrDefault(ti => ti is KubernetesTelemetryInitializer);
            Assert.NotNull(targetTelemetryInitializer);

            // K8s services	
            serviceProvider.GetRequiredService<IKubeHttpClientSettingsProvider>();
            serviceProvider.GetRequiredService<KubeHttpClientFactory>();
            serviceProvider.GetRequiredService<K8sQueryClientFactory>();
            serviceProvider.GetRequiredService<IK8sEnvironmentFactory>();
        }

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

        [Fact(DisplayName = "Set timeout through configuration works for telemetry initializer.")]
        public void EnableAppInsightsForKubernetesWithTimeOutSetThroughConfiguration()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>(){
                    {"a" , "b"},
                    {"AppInsightsForKubernetes:InitializationTimeout", "3.1:12:15.34"}
            }).Build();
            services.AddSingleton(config);

            ApplicationInsightsExtensions.InjectKubernetesTelemetryInitializer(services, () => true, new KubernetesTestServiceCollectionBuilder(), null);
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

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

        [Fact(DisplayName = "Options by code takes precedance of configuration.")]
        public void EnableAppInsightsForKubernetesWithTimeOutSetThroughOptionsOverwritingConfiugure()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>(){
                    {"a" , "b"},
                    {"AppInsightsForKubernetes:InitializationTimeout", "3.1:12:15.34"}
            }).Build();
            services.AddSingleton(config);

            ApplicationInsightsExtensions.InjectKubernetesTelemetryInitializer(services, () => true, new KubernetesTestServiceCollectionBuilder(),
                option =>
                {
                    option.InitializationTimeout = TimeSpan.FromSeconds(30);
                });
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

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
            TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration();
            telemetryConfiguration.AddApplicationInsightsKubernetesEnricher(null, new KubernetesTestServiceCollectionBuilder(), () => true);
            Assert.NotNull(telemetryConfiguration.TelemetryInitializers);
            Assert.Single(telemetryConfiguration.TelemetryInitializers);
            Assert.True(telemetryConfiguration.TelemetryInitializers.First() is KubernetesTelemetryInitializer);
        }
    }
}
