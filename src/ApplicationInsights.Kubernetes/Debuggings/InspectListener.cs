using System;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// The default listener of Application Insights for Kubernetes.
    /// </summary>
    public sealed class InspectListener
    {
        private InspectLevel _minimumLevel = InspectLevel.Warning;
        private InspectListener() { }
        /// <summary>
        /// Gets the singleton instance of Application Insights for Kubernetes diagnostic source listener
        /// </summary>
        /// <returns></returns>
        public static InspectListener Instance { get; } = new InspectListener();

        /// <summary>
        /// Sets the minimum level of the logs to show.
        /// </summary>
        public void SetMinimumLevel(InspectLevel newLevel) => _minimumLevel = newLevel;

#pragma warning disable CA1822
        /// <summary>
        /// Invokes the message of critical.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(nameof(InspectLevel.Critical))]
        public void OnLogCritical(string content)
        {
            WriteLine(InspectLevel.Critical, content);
        }

        /// <summary>
        /// Invokes the message of error.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(nameof(InspectLevel.Error))]
        public void OnLogError(string content)
        {
            WriteLine(InspectLevel.Error, content);
        }

        /// <summary>
        /// Invokes the message of warning.
        /// </summary>
        /// <param name="content"></param>
        [DiagnosticName(nameof(InspectLevel.Warning))]
        public void OnLogWarning(string content)
        {
            WriteLine(InspectLevel.Warning, content);
        }

        /// <summary>
        /// Invokes the message of information.
        /// </summary>
        [DiagnosticName(nameof(InspectLevel.Information))]
        public void OnLogInfo(string content)
        {
            WriteLine(InspectLevel.Information, content);
        }

        /// <summary>
        /// Invokes the message of debugging.
        /// </summary>
        [DiagnosticName(nameof(InspectLevel.Debug))]
        public void OnLogDebug(string content)
        {
            WriteLine(InspectLevel.Debug, content);
        }

        /// <summary>
        /// Invokes the message of tracing.
        /// </summary>
        [DiagnosticName(nameof(InspectLevel.Trace))]
        public void OnLogTrace(string content)
        {
            WriteLine(InspectLevel.Trace, content);
        }
#pragma warning restore CA1822

        private void WriteLine(InspectLevel level, string content)
        {
            if (level >= _minimumLevel)
            {
                Console.WriteLine($"[{level}] {content}");
            }
        }
    }
}