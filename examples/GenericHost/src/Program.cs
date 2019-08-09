using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace AIK8sGenericHost
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            // Channel is explicitly configured to do flush on it later.
            var channel = new InMemoryChannel();

            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Uncomment the following lines for debugging AI.K8s.
                    // Refer to https://github.com/microsoft/ApplicationInsights-Kubernetes/blob/develop/docs/SelfDiagnostics.MD for details.
                    // var observer = new ApplicationInsightsKubernetesDiagnosticObserver(DiagnosticLogLevel.Trace);
                    // ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);

                    // Add application insights for Kubernetes. Making sure this is called before services.Configure<TelemetryConfiguration>().
                    services.AddApplicationInsightsKubernetesEnricher();

                    services.Configure<TelemetryConfiguration>(
                        (config) =>
                        {
                            config.TelemetryChannel = channel;
                        }
                    );

                    // Add the logging pipelines to use. We are using Application Insights only here.
                    services.AddLogging(builder =>
                        {
                            // Optional: Apply filters to configure LogLevel Trace or above is sent to
                            // Application Insights for all categories.
                            builder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>
                                                            ("", LogLevel.Trace);
                            builder.AddApplicationInsights("----Your instrumentation key----");
                            builder.AddConsole();
                        });

                    // Register your services that implemented IHostedService interface. For example, SendAIEventService
                    services.AddHostedService<SendAIEventService>();
                }).Build();

            IApplicationLifetime lifetime = host.Services.GetRequiredService<IApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                channel.Flush();
                // Work around Application Insights issue:
                // https://github.com/Microsoft/ApplicationInsights-dotnet/issues/407
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            });
            await host.RunAsync();
        }
    }
}
