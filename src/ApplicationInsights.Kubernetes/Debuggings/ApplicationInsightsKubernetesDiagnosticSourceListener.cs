using System;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// The default listener of Application Insights for Kubernetes.
    /// </summary>
    public sealed class ApplicationInsightsKubernetesDiagnosticSourceConsoleListener
    {

        private ApplicationInsightsKubernetesDiagnosticSourceConsoleListener() { }
        /// <summary>
        /// Gets the singleton instance of Application Insights for Kubernetes diagnostic source listener
        /// </summary>
        /// <returns></returns>
        public static ApplicationInsightsKubernetesDiagnosticSourceConsoleListener Instance { get; } = new ApplicationInsightsKubernetesDiagnosticSourceConsoleListener();

#pragma warning disable CA1822
        /// <summary>
        /// Invokes the message of critical.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSourceLevel.Critical)]
        public void OnLogCritical(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Critical, content);
        }

        /// <summary>
        /// Invokes the message of error.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSourceLevel.Error)]
        public void OnLogError(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Error, content);
        }

        /// <summary>
        /// Invokes the message of warning.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSourceLevel.Warning)]
        public void OnLogWarning(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Warning, content);
        }

        /// <summary>
        /// Invokes the message of information.
        /// </summary>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSourceLevel.Information)]
        public void OnLogInfo(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Information, content);
        }

        /// <summary>
        /// Invokes the message of debugging.
        /// </summary>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSourceLevel.Debug)]
        public void OnLogDebug(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Debug, content);
        }

        /// <summary>
        /// Invokes the message of tracing.
        /// </summary>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSourceLevel.Trace)]
        public void OnLogTrace(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Trace, content);
        }
#pragma warning restore CA1822
        private static void WriteLine(string level, string content)
        {
            Console.WriteLine($"[{level}]::{content}");
        }
    }
}