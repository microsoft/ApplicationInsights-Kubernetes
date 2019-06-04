using System;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// The default listener of Application Insights for Kubernetes.
    /// </summary>
    public sealed class ApplicationInsightsKubernetesDiagnosticSourceConsoleListener
    {
        private ApplicationInsightsKubernetesDiagnosticSourceLevel _minimumLevel = ApplicationInsightsKubernetesDiagnosticSourceLevel.Warning;
        private ApplicationInsightsKubernetesDiagnosticSourceConsoleListener() { }
        /// <summary>
        /// Gets the singleton instance of Application Insights for Kubernetes diagnostic source listener
        /// </summary>
        /// <returns></returns>
        public static ApplicationInsightsKubernetesDiagnosticSourceConsoleListener Instance { get; } = new ApplicationInsightsKubernetesDiagnosticSourceConsoleListener();

        /// <summary>
        /// Sets the minimum level of the logs to show.
        /// </summary>
        public void SetMinimumLevel(ApplicationInsightsKubernetesDiagnosticSourceLevel newLevel) => _minimumLevel = newLevel;

#pragma warning disable CA1822
        /// <summary>
        /// Invokes the message of critical.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(nameof(ApplicationInsightsKubernetesDiagnosticSourceLevel.Critical))]
        public void OnLogCritical(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Critical, content);
        }

        /// <summary>
        /// Invokes the message of error.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(nameof(ApplicationInsightsKubernetesDiagnosticSourceLevel.Error))]
        public void OnLogError(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Error, content);
        }

        /// <summary>
        /// Invokes the message of warning.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(nameof(ApplicationInsightsKubernetesDiagnosticSourceLevel.Warning))]
        public void OnLogWarning(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Warning, content);
        }

        /// <summary>
        /// Invokes the message of information.
        /// </summary>
        [DiagnosticName(nameof(ApplicationInsightsKubernetesDiagnosticSourceLevel.Information))]
        public void OnLogInfo(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Information, content);
        }

        /// <summary>
        /// Invokes the message of debugging.
        /// </summary>
        [DiagnosticName(nameof(ApplicationInsightsKubernetesDiagnosticSourceLevel.Debug))]
        public void OnLogDebug(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Debug, content);
        }

        /// <summary>
        /// Invokes the message of tracing.
        /// </summary>
        [DiagnosticName(nameof(ApplicationInsightsKubernetesDiagnosticSourceLevel.Trace))]
        public void OnLogTrace(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel.Trace, content);
        }
#pragma warning restore CA1822

        private void WriteLine(ApplicationInsightsKubernetesDiagnosticSourceLevel level, string content)
        {
            if (level >= _minimumLevel)
            {
                Console.WriteLine($"[{level}] {content}");
            }
        }
    }
}