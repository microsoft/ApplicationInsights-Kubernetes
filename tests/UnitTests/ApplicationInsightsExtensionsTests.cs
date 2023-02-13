using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

/// <summary>
/// Test ApplicationInsightsExtensions. The goal for this unit test is to enforce there being
/// proper overloads for the user to call on the IServiceCollection.
/// </summary>
[Collection(FullLoggingCollection.Name)]
public class ApplicationInsightsExtensionsTests
{
    [Fact]
    public void ShouldAllowParameterless()
    {
        IServiceCollection collection = new ServiceCollection();

        // If there's compile error, check if the signature of parameterless of AddApplicationInsightsKubernetesEnricher is broken.
        collection = collection.AddApplicationInsightsKubernetesEnricher();

        Assert.NotNull(collection);
    }

    [Fact]
    public void ShouldAllowOverwriteOptions()
    {
        IServiceCollection collection = new ServiceCollection();

        // If there's compile error, check if the signature of AddApplicationInsightsKubernetesEnricher was changed.
        collection = collection.AddApplicationInsightsKubernetesEnricher(applyOptions: opt => opt.InitializationTimeout = TimeSpan.FromMinutes(15));

        Assert.NotNull(collection);
    }

    [Fact]
    public void ShouldAllowOverwriteDiagnosticLoggingLevel()
    {
        IServiceCollection collection = new ServiceCollection();

        // If there's compile error, check if the signature of AddApplicationInsightsKubernetesEnricher was changed.
        collection = collection.AddApplicationInsightsKubernetesEnricher(diagnosticLogLevel: LogLevel.Trace);

        Assert.NotNull(collection);
    }

    [Fact]
    public void ShouldAllowOverwritingOptionsAndDiagnosticLoggingLevel()
    {
        IServiceCollection collection = new ServiceCollection();

        // If there's compile error, check if the signature of AddApplicationInsightsKubernetesEnricher was changed.
        collection = collection.AddApplicationInsightsKubernetesEnricher(
            diagnosticLogLevel: LogLevel.Trace,
            applyOptions: opt => opt.InitializationTimeout = TimeSpan.FromMinutes(15));

        Assert.NotNull(collection);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void ShouldNotRegisterHostedServiceWhenSet(bool disableBackgroundService, bool expectServiceRegistered)
    {
        IServiceCollection collection = new ServiceCollection();

        Mock<IClusterEnvironmentCheck> clusterCheck = new();
        clusterCheck.Setup(c => c.IsInCluster).Returns(true);

        // If there's compile error, check if the signature of AddApplicationInsightsKubernetesEnricher was changed.
        collection = collection.AddApplicationInsightsKubernetesEnricher(disableBackgroundService: disableBackgroundService, clusterCheck: clusterCheck.Object);

        Assert.NotNull(collection);
        bool registered = collection.Any(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IHostedService));

        if (expectServiceRegistered)
        {
            Assert.True(registered);
        }
        else
        {
            Assert.False(registered);
        }
    }
}
