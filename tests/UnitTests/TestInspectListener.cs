using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal sealed class TestInspectListener
    {
        public int GetCount(InspectLevel level)
        {
            return Output.Where(o => o.Item1 == level).Count();
        }

        public List<(InspectLevel, string)> Output { get; } = new List<(InspectLevel, string)>();

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
            Output.Add((level, content));
        }
    }
}
