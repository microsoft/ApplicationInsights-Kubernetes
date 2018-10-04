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
        Timer _asyncTimer;
        public PrintHelloService(TelemetryClient telemetryClient)
        {
            this._client = telemetryClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _asyncTimer = new Timer(state =>
            {
                string message = "Hello Hosted Service!!!";
                Console.WriteLine(message);
                this._client.TrackEvent(new EventTelemetry(message));
                this._client.Flush();
            }, null, 0, 30 * 1000);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_asyncTimer != null)
            {
                _asyncTimer.Dispose();
                _asyncTimer = null;
            }
            return Task.CompletedTask;
        }
    }
}