using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApplicationInsights.Kubernetes.Examples
{
    internal class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;

        public Worker(ILogger<Worker> logger, TelemetryClient telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (_telemetryClient.StartOperation<RequestTelemetry>("Example operation"))
                {
                    _telemetryClient.TrackEvent("A custom event!");
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
