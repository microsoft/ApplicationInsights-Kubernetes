using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;

namespace HostBuilderExample
{
    public class PrintHelloService : IHostedService
    {
        TelemetryClient _client;
        public PrintHelloService(TelemetryClient telemetryClient)
        {
            this._client = telemetryClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            Console.WriteLine("Hello Hosted Service!!");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}