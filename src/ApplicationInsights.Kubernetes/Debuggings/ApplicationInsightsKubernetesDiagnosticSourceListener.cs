using Microsoft.Extensions.DiagnosticAdapter;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    public class ApplicationInsightsKubernetesDiagnosticSourceConsoleListener
    {
        [DiagnosticName(ApplicationInsightsKubernetesDiagnosticSource.DiagnosticSourceName + "." + ApplicationInsightsKubernetesDiagnosticSource.Level.Information)]
        public void OnLogInfo(string content)
        {
            WriteLine(ApplicationInsightsKubernetesDiagnosticSource.Level.Information, content);
        }

        private void WriteLine(string level, string content)
        {
            System.Console.WriteLine($"[{level}] {content}");
        }
    }
}