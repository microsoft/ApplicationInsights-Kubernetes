using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;
public class KubernetesServiceCollectionBuilderTests
{
    [Fact]
    public void AllServicesAreRegistered()
    {
        using IHost host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            Mock<IClusterEnvironmentCheck> clusterCheckMock = new();

            clusterCheckMock.Setup(c => c.IsInCluster).Returns(true);

            // All required services are registered.
            services.AddApplicationInsightsKubernetesEnricher(
                applyOptions: null,
                diagnosticLogLevel: LogLevel.None,
                clusterCheck: clusterCheckMock.Object);

            // Hack so that no real cluster is contacted
            services.AddSingleton<IK8sClientService>(_ =>
            {
                return new Mock<IK8sClientService>().Object;
            });

            using (ServiceProvider serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions()
            {
                ValidateScopes = false, // Reduce noise.
                ValidateOnBuild = true, // Making sure all services are resolvable.
            }))
            {
                Assert.NotNull(serviceProvider);    // Exception thrown when missing service on dependency graph.
            }
        }).Build();
    }

    [Fact]
    public void AllServicesAreRegisteredWithScopeValidation()
    {
        using IHost host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            Mock<IClusterEnvironmentCheck> clusterCheckMock = new();

            clusterCheckMock.Setup(c => c.IsInCluster).Returns(true);

            // All required services are registered.
            services.AddApplicationInsightsKubernetesEnricher(
                applyOptions: null,
                diagnosticLogLevel: LogLevel.None,
                clusterCheck: clusterCheckMock.Object);

            // Hack so that no real cluster is contacted
            services.AddSingleton<IK8sClientService>(_ =>
            {
                return new Mock<IK8sClientService>().Object;
            });

            using (ServiceProvider serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions()
            {
                ValidateScopes = true, // Validate the lifetime.
                ValidateOnBuild = true, // Making sure all services are resolvable.
            }))
            {
                Assert.NotNull(serviceProvider);    // Exception thrown when missing service on dependency graph.
            }
        }).Build();
    }

    // Regression test to remain.
    [Fact]
    public void ContainerIdHolderShallNotCaptureScopedServices()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddApplicationInsightsKubernetesEnricher(clusterCheck: AlwaysInClusterCheck.Instance);

        using (ServiceProvider sp = services.BuildServiceProvider(validateScopes: true))
        {
            // Container id holder shall be constructed without scope validation error.
            IContainerIdHolder containerIdHolder = sp.GetRequiredService<IContainerIdHolder>();
            // Pass when there has no exception at this point.
        }
    }

    /// <summary>
    /// There are services that requested by scope factory that can't be verified on building service provider.
    /// Verify them in this unit test.
    /// </summary>
    [Fact]
    public void VerifyLoseServiceAreResolvable()
    {
        using IHost host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            Mock<IClusterEnvironmentCheck> clusterCheckMock = new();

            clusterCheckMock.Setup(c => c.IsInCluster).Returns(true);

            // All required services are registered.
            services.AddApplicationInsightsKubernetesEnricher(
                applyOptions: null,
                diagnosticLogLevel: LogLevel.None,
                clusterCheck: clusterCheckMock.Object);

            // Hack so that no real cluster is contacted
            services.AddSingleton<IK8sClientService>(_ =>
            {
                return new Mock<IK8sClientService>().Object;
            });

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                // IK8sEnvironmentFetcher is used by K8sInfoBackgroundService
                IK8sEnvironmentFetcher k8sEnvironmentFetcher = serviceProvider.GetRequiredService<IK8sEnvironmentFetcher>();
                Assert.NotNull(k8sEnvironmentFetcher);

                // IContainerIdNormalizer is used by ContainerIdHolder
                IContainerIdNormalizer containerIdNormalizer = serviceProvider.GetRequiredService<IContainerIdNormalizer>();
                Assert.NotNull(containerIdNormalizer);

                // IContainerIdNormalizer is used by IContainerIdProviders
                IEnumerable<IContainerIdProvider> containerIdProviders = serviceProvider.GetRequiredService<IEnumerable<IContainerIdProvider>>();
                Assert.NotNull(containerIdProviders);
                Assert.True(containerIdProviders.Any());
            }
        }).Build();
    }
}
