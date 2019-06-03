using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// The default listener of Application Insights for Kubernetes.
    /// </summary>
    public class ApplicationInsightsKubernetesDiagnosticSourceConsoleListener
    {
        /// <summary>
        /// Invokes the message of critical.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSource.Level.Critical)]
        public void OnLogCritical(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSource.Level.Critical, content);
        }

        /// <summary>
        /// Invokes the message of error.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSource.Level.Error)]
        public void OnLogError(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSource.Level.Error, content);
        }

        /// <summary>
        /// Invokes the message of warning.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSource.Level.Warning)]
        public void OnLogWarning(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSource.Level.Warning, content);
        }

        /// <summary>
        /// Invokes the message of information.
        /// </summary>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSource.Level.Information)]
        public void OnLogInfo(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSource.Level.Information, content);
        }

        /// <summary>
        /// Invokes the message of debugging.
        /// </summary>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSource.Level.Debug)]
        public void OnLogDebug(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSource.Level.Debug, content);
        }

        /// <summary>
        /// Invokes the message of tracing.
        /// </summary>
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSource.Level.Trace)]
        public void OnLogTrace(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSource.Level.Trace, content);
        }

        private void WriteLine(string level, string content)
        {
            System.Console.WriteLine($"[{level}] {content}");
        }
    }
}