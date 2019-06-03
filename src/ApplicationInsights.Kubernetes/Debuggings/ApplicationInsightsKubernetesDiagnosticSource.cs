using System.Diagnostics;
using System.Globalization;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Diagnostic Source of Application Insights for Kubernetes.
    /// </summary>
    public sealed class ApplicationInsightsKubernetesDiagnosticSource
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

        private ApplicationInsightsKubernetesDiagnosticSource()
        {
            _innerSource = new DiagnosticListener(DiagnosticSourceName);
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
            Write(Level.Critical, message, args);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        public void LogError(string message, params object[] args)
        {
            Write(Level.Error, message, args);
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        public void LogWarning(string message, params object[] args)
        {
            Write(Level.Warning, message, args);
        }

        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogInformation(string message, params object[] args)
        {
            Write(Level.Information, message, args);
        }

        /// <summary>
        /// Logs the debugging message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogDebug(string message, params object[] args)
        {
            Write(Level.Debug, message, args);
        }

        /// <summary>
        /// Logs the trace message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void LogTrace(string message, params object[] args)
        {
            Write(Level.Trace, message, args);
        }

        /// <summary>
        /// Diagnostic Source message levels
        /// </summary>
        public static class Level
        {
            /// <summary>
            /// Gets the level of critical.
            /// </summary>
            /// <returns></returns>
            public const string Critical = nameof(Critical);
            /// <summary>
            /// Gets the level of error.
            /// </summary>
            /// <returns></returns>
            public const string Error = nameof(Error);
            /// <summary>
            /// Gets the level of warning.
            /// </summary>
            /// <returns></returns>
            public const string Warning = nameof(Warning);
            /// <summary>
            /// Gets the level of information.
            /// </summary>
            /// <returns></returns>
            public const string Information = nameof(Information);
            /// <summary>
            /// Gets the level of debug.
            /// </summary>
            /// <returns></returns>
            public const string Debug = nameof(Debug);
            /// <summary>
            /// Gets the level of trace.
            /// </summary>
            /// <returns></returns>
            public const string Trace = nameof(Trace);
        }

        private void Write(string level, string message, params object[] args)
        {
            if (_innerSource.IsEnabled(level))
            {
                _innerSource.Write(level, new
                {
                    content = string.Format(CultureInfo.InvariantCulture, message, args),
                });
            }
        }
    }
}