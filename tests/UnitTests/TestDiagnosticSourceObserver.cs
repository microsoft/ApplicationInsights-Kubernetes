using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

internal sealed class TestDiagnosticSourceObserver
{
    public int GetCount(DiagnosticLogLevel level)
    {
        return Output.Where(o => o.Item1 == level).Count();
    }

    public List<(DiagnosticLogLevel, string)> Output { get; } = new List<(DiagnosticLogLevel, string)>();

    /// <summary>
    /// Invokes the message of critical.
    /// </summary>
    /// <param name="content"></param>
    [DiagnosticName(nameof(DiagnosticLogLevel.Critical))]
    public void OnLogCritical(string content)
    {
        WriteLine(DiagnosticLogLevel.Critical, content);
    }

    /// <summary>
    /// Invokes the message of error.
    /// </summary>
    /// <param name="content"></param>
    [DiagnosticName(nameof(DiagnosticLogLevel.Error))]
    public void OnLogError(string content)
    {
        WriteLine(DiagnosticLogLevel.Error, content);
    }

    /// <summary>
    /// Invokes the message of warning.
    /// </summary>
    /// <param name="content"></param>
    [DiagnosticName(nameof(DiagnosticLogLevel.Warning))]
    public void OnLogWarning(string content)
    {
        WriteLine(DiagnosticLogLevel.Warning, content);
    }

    /// <summary>
    /// Invokes the message of information.
    /// </summary>
    [DiagnosticName(nameof(DiagnosticLogLevel.Information))]
    public void OnLogInfo(string content)
    {
        WriteLine(DiagnosticLogLevel.Information, content);
    }

    /// <summary>
    /// Invokes the message of debugging.
    /// </summary>
    [DiagnosticName(nameof(DiagnosticLogLevel.Debug))]
    public void OnLogDebug(string content)
    {
        WriteLine(DiagnosticLogLevel.Debug, content);
    }

    /// <summary>
    /// Invokes the message of tracing.
    /// </summary>
    [DiagnosticName(nameof(DiagnosticLogLevel.Trace))]
    public void OnLogTrace(string content)
    {
        WriteLine(DiagnosticLogLevel.Trace, content);
    }
#pragma warning restore CA1822

    private void WriteLine(DiagnosticLogLevel level, string content)
    {
        Output.Add((level, content));
    }
}
