using System.Diagnostics;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

public class LoggingLevelFixture
{
    public LoggingLevelFixture()
    {
        // Making sure the the logging source is observed in case there's bugs in the logging code.
        ApplicationInsightsKubernetesDiagnosticObserver observer = new ApplicationInsightsKubernetesDiagnosticObserver(DiagnosticLogLevel.Trace);
        ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);
    }
}

[CollectionDefinition(FullLoggingCollection.Name)]
public class FullLoggingCollection : ICollectionFixture<LoggingLevelFixture>
{
    public const string Name = $"{nameof(LoggingLevelFixture)}Collection";
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
