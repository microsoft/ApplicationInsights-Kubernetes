using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIK8sGenericHost
{
    public class SendAIEventService : IHostedService
    {
        private readonly ILogger _logger;

        public SendAIEventService(ILogger<SendAIEventService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task _ = Task.Run(async () => await RunUntilCancelledAsync(cancellationToken));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task RunUntilCancelledAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Application Insights for Kubernetes is working . . .");
                await Task.Delay(5000).ConfigureAwait(false);
            }
        }
    }
}