// using System.Diagnostics;
// using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApplicationInsights.Kubernetes.Examples;

class Program
{

    static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Uncomment the following lines for debugging AI.K8s.
                // Refer to https://github.com/microsoft/ApplicationInsights-Kubernetes/blob/develop/docs/SelfDiagnostics.MD for details.
                // var observer = new ApplicationInsightsKubernetesDiagnosticObserver(DiagnosticLogLevel.Trace);
                // ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);

                services.AddApplicationInsightsTelemetryWorkerService();
                services.AddApplicationInsightsKubernetesEnricher();
                services.AddHostedService<Worker>();
            });
}