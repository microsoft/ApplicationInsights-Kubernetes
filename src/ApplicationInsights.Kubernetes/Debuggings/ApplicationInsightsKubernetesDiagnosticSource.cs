using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Diagnostic Source of Application Insights for Kubernetes.
    /// </summary>
    public sealed class ApplicationInsightsKubernetesDiagnosticSource
    {
        private readonly DiagnosticSource _innerSource;

        /// <summary>
        /// Diagnostic source listener subscribe point.
        /// </summary>
        public DiagnosticListener Observable => (DiagnosticListener)this._innerSource;

        /// <summary>
        /// Gets the name of the diagnostic source.
        /// </summary>
        public const string DiagnosticSourceName = "ApplicationInsightsKubernetesDiagnosticSource";

        private ApplicationInsightsKubernetesDiagnosticSource()
        {
            _innerSource = new DiagnosticListener(DiagnosticSourceName);
        }

        // Explict static constructor to tell C# compiler not to mark type as beforefieldinit
        static ApplicationInsightsKubernetesDiagnosticSource()
        {
        }

        /// <summary>
        /// Gets the singleton instance of Application Insights for Kubernetes.
        /// </summary>
        /// <returns></returns>
        public static ApplicationInsightsKubernetesDiagnosticSource Instance { get; } = new ApplicationInsightsKubernetesDiagnosticSource();

        /// <summary>
        /// Logs the critical message.
        /// </summary>
        public void LogCritical(string message, params object[] args)
        {
            Write(DiagnosticLogLevel.Critical, message, args);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        public void LogError(string message, params object[] args)
        {
            Write(DiagnosticLogLevel.Error, message, args);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        public void LogWarning(string message, params object[] args)
        {
            Write(DiagnosticLogLevel.Warning, message, args);
        }

        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogInformation(string message, params object[] args)
        {
            Write(DiagnosticLogLevel.Information, message, args);
        }

        /// <summary>
        /// Logs the debugging message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogDebug(string message, params object[] args)
        {
            Write(DiagnosticLogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs the trace message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogTrace(string message, params object[] args)
        {
            Write(DiagnosticLogLevel.Trace, message, args);
        }

        private void Write(DiagnosticLogLevel level, string message, params object[] args)
        {
            if (_innerSource.IsEnabled(level.ToString()))
            {
                string timedFormat = "[" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) + "] " + message;
                _innerSource.Write(level.ToString(), new
                {
                    content = string.Format(CultureInfo.InvariantCulture, timedFormat, args),
                });
            }
        }
    }
}