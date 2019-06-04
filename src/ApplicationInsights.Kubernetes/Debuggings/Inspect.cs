using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Diagnostic Source of Application Insights for Kubernetes.
    /// </summary>
    public sealed class Inspect
    {
        private DiagnosticSource _innerSource { get; }

        /// <summary>
        /// Diagnostic source listener subscribe point.
        /// </summary>
        /// <returns></returns>
        public DiagnosticListener Observer => (DiagnosticListener)this._innerSource;

        /// <summary>
        /// Gets the name of the diagnostic source.
        /// </summary>
        public const string DiagnosticSourceName = "ApplicationInsightsKubernetesDiagnosticSource";

        private Inspect()
        {
            _innerSource = new DiagnosticListener(DiagnosticSourceName);
        }

        /// <summary>
        /// Gets the singleton instance of Application Insights for Kubernetes.
        /// </summary>
        /// <returns></returns>
        public static Inspect Instance { get; } = new Inspect();

        /// <summary>
        /// Logs the critical message.
        /// </summary>
        public void LogCritical(string message, params object[] args)
        {
            Write(InspectLevel.Critical, message, args);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        public void LogError(string message, params object[] args)
        {
            Write(InspectLevel.Error, message, args);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        public void LogWarning(string message, params object[] args)
        {
            Write(InspectLevel.Warning, message, args);
        }

        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogInformation(string message, params object[] args)
        {
            Write(InspectLevel.Information, message, args);
        }

        /// <summary>
        /// Logs the debugging message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogDebug(string message, params object[] args)
        {
            Write(InspectLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs the trace message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogTrace(string message, params object[] args)
        {
            Write(InspectLevel.Trace, message, args);
        }

        private void Write(InspectLevel level, string message, params object[] args)
        {
            if (_innerSource.IsEnabled(level.ToString()))
            {
                _innerSource.Write(level.ToString(), new
                {
                    content = string.Format(CultureInfo.InvariantCulture, message, args),
                });
            }
        }

    }
}