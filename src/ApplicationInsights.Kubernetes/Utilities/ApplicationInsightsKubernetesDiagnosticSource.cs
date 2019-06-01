using System.Diagnostics;
using System.Globalization;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    internal sealed class ApplicationInsightsKubernetesDiagnosticSource
    {
        private DiagnosticSource _inner;
        public const string DiagnosticSourceName = "ApplicationInsightsKubernetesDiagnosticSource";

        private ApplicationInsightsKubernetesDiagnosticSource()
        {
            _inner = new DiagnosticListener(DiagnosticSourceName);
        }
        public static ApplicationInsightsKubernetesDiagnosticSource Instance { get; } = new ApplicationInsightsKubernetesDiagnosticSource();

        public void LogCritical(string message, params object[] args)
        {
            Write(Level.Critical, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            Write(Level.Error, message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            Write(Level.Warning, message, args);
        }

        public void LogInformation(string message, params object[] args)
        {
            Write(Level.Information, message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            Write(Level.Debug, message, args);
        }

        public void LogTrace(string message, params object[] args)
        {
            Write(Level.Trace, message, args);
        }

        private void Write(string level, string message, params object[] args)
        {
            if (_inner.IsEnabled(level))
            {
                _inner.Write(level, new
                {
                    content = string.Format(CultureInfo.InvariantCulture, message, args),
                });
            }
        }

        public static class Level
        {
            public const string Critical = nameof(Critical);
            public const string Error = nameof(Error);
            public const string Warning = nameof(Warning);
            public const string Information = nameof(Information);
            public const string Debug = nameof(Debug);
            public const string Trace = nameof(Trace);
        }
    }
}