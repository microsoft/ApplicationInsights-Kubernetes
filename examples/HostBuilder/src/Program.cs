using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights;

namespace HostBuilderExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder().ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables();
                }).ConfigureServices((hostContext, services) =>
                {
                    services.AddApplicationInsightsTelemetry(options =>{
                        options.InstrumentationKey="9558b686-2a23-441c-b389-37b04312c4ad";
                        options.
                    });
                    // Enable Application Insights with iKey configured in the environemnt variable
                    services.AddSingleton<IHostedService, PrintHelloService>();
                    // Enable Application Insights for Kubernetes when running inside Kubernetes.
                    services.EnableKubernetes();
                })
                .UseConsoleLifetime()
                .Build();

            host.Run();
        }
    }
}
