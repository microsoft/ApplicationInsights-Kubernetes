using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        collection = collection.AddApplicationInsightsKubernetesEnricher(opt => opt.InitializationTimeout = TimeSpan.FromMinutes(15));

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
            opt => opt.InitializationTimeout = TimeSpan.FromMinutes(15),
            diagnosticLogLevel: LogLevel.Trace);

        Assert.NotNull(collection);
    }
}
