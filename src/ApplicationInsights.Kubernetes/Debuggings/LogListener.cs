using System;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// The default listener of Application Insights for Kubernetes.
    /// </summary>
    public sealed class LogListener
    {
        private DiagnosticLogLevel _minimumLevel = DiagnosticLogLevel.Warning;
        private LogListener() { }
        /// <summary>
        /// Gets the singleton instance of Application Insights for Kubernetes diagnostic source listener
        /// </summary>
        /// <returns></returns>
        public static LogListener Instance { get; } = new LogListener();

        /// <summary>
        /// Sets the minimum level of the logs to show.
        /// </summary>
        public void SetMinimumLevel(DiagnosticLogLevel newLevel) => _minimumLevel = newLevel;

#pragma warning disable CA1822
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
            if (level >= _minimumLevel)
            {
                Console.WriteLine($"[{level}] {content}");
            }
        }
    }
}