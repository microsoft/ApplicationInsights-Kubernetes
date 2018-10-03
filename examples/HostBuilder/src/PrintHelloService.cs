using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
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
            string message = "Hello Hosted Service!!!";
            Console.WriteLine(message);
            this._client.TrackEvent(new EventTelemetry(message));
            this._client.Flush();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}