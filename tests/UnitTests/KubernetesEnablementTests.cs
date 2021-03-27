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
        public void ServicesRegistered()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(p => new ConfigurationBuilder().Build());

            ApplicationInsightsExtensions.ConfigureKubernetesTelemetryInitializer(services, () => true, new KubernetesTestServiceCollectionBuilder(), null);
            Assert.NotNull(services.FirstOrDefault(sd => sd.ImplementationType == typeof(KubernetesTelemetryInitializer)));

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ITelemetryInitializer targetTelemetryInitializer = serviceProvider.GetServices<ITelemetryInitializer>().First(ti => ti is KubernetesTelemetryInitializer);
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
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(p => new ConfigurationBuilder().Build());

            serviceCollection.AddApplicationInsightsKubernetesEnricher(
                applyOptions: null,
                kubernetesServiceCollectionBuilder: new KubernetesTestServiceCollectionBuilder(),
                detectKubernetes: () => true);
            ITelemetryInitializer targetTelemetryInitializer = serviceCollection.BuildServiceProvider().GetServices<ITelemetryInitializer>().First(i => i is KubernetesTelemetryInitializer);

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
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(p => new ConfigurationBuilder().Build());
            serviceCollection.AddApplicationInsightsKubernetesEnricher(
                    applyOptions: option =>
                    {
                        option.InitializationTimeout = TimeSpan.FromSeconds(5);
                    },
                    kubernetesServiceCollectionBuilder: new KubernetesTestServiceCollectionBuilder(),
                    detectKubernetes: () => true);
            ITelemetryInitializer targetTelemetryInitializer = serviceCollection.BuildServiceProvider().GetServices<ITelemetryInitializer>().First(i => i is KubernetesTelemetryInitializer);

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

            ApplicationInsightsExtensions.ConfigureKubernetesTelemetryInitializer(services, () => true, new KubernetesTestServiceCollectionBuilder(), null);
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

        [Fact(DisplayName = "Options by code takes precedence of configuration.")]
        public void EnableAppInsightsForKubernetesWithTimeOutSetThroughOptionsOverwritingConfiugure()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>(){
                    {"a" , "b"},
                    {"AppInsightsForKubernetes:InitializationTimeout", "3.1:12:15.34"}
            }).Build();
            services.AddSingleton(config);

            ApplicationInsightsExtensions.ConfigureKubernetesTelemetryInitializer(services, () => true, new KubernetesTestServiceCollectionBuilder(),
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
    }
}
