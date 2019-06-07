using System;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiK8sConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            string iKey = config["APPINSIGHTS_INSTRUMENTNATIONKEY"];
            if (string.IsNullOrEmpty(iKey))
            {
                Console.WriteLine("Can't do empty iKey.");
            }

            using (TelemetryConfiguration configuration = new TelemetryConfiguration(iKey))
            {
                configuration.AddApplicationInsightsKubernetesEnricher(applyOptions: null);

                TelemetryClient client = new TelemetryClient(configuration);
                Console.WriteLine("Sending trace telemetry once a while.");
                while (true)
                {
                    client.TrackTrace("Hello from AI SDK");
                    client.Flush();
                    Thread.Sleep(30000);
                }
            }
        }
    }
}
