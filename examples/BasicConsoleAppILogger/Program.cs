// using System.Diagnostics;
// using Microsoft.ApplicationInsights.Kubernetes.Debugging;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace BasicConsoleAppILogger
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using ITelemetryChannel channel = new InMemoryChannel();

            try
            {
                // Create the DI container.
                IServiceCollection services = new ServiceCollection();

                // Channel is explicitly configured to do flush on it later.
                services.AddOptions<TelemetryConfiguration>().Configure<IServiceProvider>((config, p) =>
                    {
                        // Appending registered telemetry initializers.
                        foreach (ITelemetryInitializer registeredInitializer in p.GetServices<ITelemetryInitializer>())
                        {
                            config.TelemetryInitializers.Add(registeredInitializer);
                        }

                        // Setup up telemetry channels.
                        config.TelemetryChannel = channel;
                    }
                );

                // Add the logging pipelines to use. We are using Application Insights only here.
                services.AddLogging(builder =>
                {
                    // Optional: Apply filters to configure LogLevel Trace or above is sent to
                    // Application Insights for all categories.
                    builder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("", LogLevel.Trace);

                    builder.AddApplicationInsights(
                        configureTelemetryConfiguration: (config) => config.ConnectionString = "InstrumentationKey=5d8258e7-abb2-4066-89a5-8c73071b74ff;IngestionEndpoint=https://westus2-0.in.applicationinsights.azure.com/;LiveEndpoint=https://westus2.livediagnostics.monitor.azure.com/",
                        configureApplicationInsightsLoggerOptions: (options) => { }
                    );

                    // Optional: Show the logs in console at the same time
                    builder.AddSimpleConsole(opt =>
                    {
                        opt.SingleLine = true;
                        opt.ColorBehavior = LoggerColorBehavior.Disabled;
                    });
                });

                // Enable K8s enricher
                services.AddSingleton<IConfiguration>(p => new ConfigurationBuilder().Build());
                services.AddApplicationInsightsKubernetesEnricher(diagnosticLogLevel: LogLevel.Debug);

                // Build ServiceProvider.
                IServiceProvider serviceProvider = services.BuildServiceProvider();
                ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                // Output logging info
                while (true)
                {
                    // Begin a new scope. This is optional.
                    using (logger.BeginScope(new Dictionary<string, object> { { "Method", nameof(Main) } }))
                    {
                        logger.LogInformation("Logger is working"); // this will be captured by Application Insights.
                    }

                    channel.Flush();
                    // Send wait for 10 seconds before going into the next iteration.
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
            finally
            {
                // Explicitly call Flush() followed by Delay, as required in console apps.
                // This ensures that even if the application terminates, telemetry is sent to the back end.
                channel.Flush();

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }
    }
}
