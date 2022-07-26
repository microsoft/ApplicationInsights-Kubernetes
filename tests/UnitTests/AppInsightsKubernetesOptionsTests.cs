using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

public class AppInsightsKubernetesOptionsTests
{
    [Fact]
    public void ShouldHaveDefaultOptions()
    {
        Mock<IClusterCheck> clusterCheck = new();

        clusterCheck.Setup(c => c.IsInCluster()).Returns(true);

        KubernetesServiceCollectionBuilder builder = new KubernetesServiceCollectionBuilder(customizeOptions: default, clusterCheck.Object);

        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = (new ConfigurationBuilder()).Build();
        services.AddSingleton<IConfiguration>(p => configuration);
        builder.RegisterServices(services);

        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            AppInsightsForKubernetesOptions options = (provider.GetRequiredService<IOptions<AppInsightsForKubernetesOptions>>()).Value;

            // Default timeout to 2 minutes
            Assert.Equal(TimeSpan.FromMinutes(2), options.InitializationTimeout);

            // No telemetry key processor
            Assert.Null(options.TelemetryKeyProcessor);
        }
    }

    [Fact]
    public void ShouldTakeOptionFromIConfiguration()
    {
        Mock<IClusterCheck> clusterCheck = new();

        clusterCheck.Setup(c => c.IsInCluster()).Returns(true);

        KubernetesServiceCollectionBuilder builder = new KubernetesServiceCollectionBuilder(customizeOptions: default, clusterCheck.Object);

        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()
        {
            [$"{AppInsightsForKubernetesOptions.SectionName}:InitializationTimeout"] = "01:00:00",
        }).Build();
        services.AddSingleton<IConfiguration>(p => configuration);
        builder.RegisterServices(services);

        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            AppInsightsForKubernetesOptions options = (provider.GetRequiredService<IOptions<AppInsightsForKubernetesOptions>>()).Value;

            Assert.Equal(TimeSpan.FromHours(1), options.InitializationTimeout);
        }
    }

    [Fact]
    public void ShouldTakeDelegateOverwriteForSettings()
    {
        Mock<IClusterCheck> clusterCheck = new();
        clusterCheck.Setup(c => c.IsInCluster()).Returns(true);

        KubernetesServiceCollectionBuilder builder = new KubernetesServiceCollectionBuilder(customizeOptions: opt =>
        {
            opt.InitializationTimeout = TimeSpan.FromSeconds(10);   // The user settings through code will take precedence.
        }, clusterCheck.Object);

        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()
        {
            [$"{AppInsightsForKubernetesOptions.SectionName}:InitializationTimeout"] = "01:00:00",  // Although set by configurations, the value is expected to be overwritten by the user code above.
        }).Build();
        services.AddSingleton<IConfiguration>(p => configuration);
        builder.RegisterServices(services);

        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            AppInsightsForKubernetesOptions options = (provider.GetRequiredService<IOptions<AppInsightsForKubernetesOptions>>()).Value;

            Assert.Equal(TimeSpan.FromSeconds(10), options.InitializationTimeout);
        }
    }

    [Fact]
    public void ShouldAllowSetTelemetryKeyProcessorByCode()
    {
        Mock<IClusterCheck> clusterCheck = new();

        clusterCheck.Setup(c => c.IsInCluster()).Returns(true);

        Func<string, string> keyTransformer = (input) => "c_" + input;

        KubernetesServiceCollectionBuilder builder = new KubernetesServiceCollectionBuilder(customizeOptions: opt =>
        {
            opt.TelemetryKeyProcessor = keyTransformer;
        }, clusterCheck.Object);

        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(p => configuration);
        builder.RegisterServices(services);

        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            AppInsightsForKubernetesOptions options = (provider.GetRequiredService<IOptions<AppInsightsForKubernetesOptions>>()).Value;

            Assert.NotNull(options.TelemetryKeyProcessor);
            Assert.Equal(keyTransformer, options.TelemetryKeyProcessor);
        }
    }
}
