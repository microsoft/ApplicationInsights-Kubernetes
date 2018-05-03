using System;
using System.Linq;
using Microsoft.ApplicationInsights.Kubernetes;
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
            services = services.EnableKubernetes();

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

        [Fact(DisplayName = "Slow Kubernetes query should not blocking the service injection.")]
        public void NoBlockingServiceInjection()
        {
            
        }
    }
}
