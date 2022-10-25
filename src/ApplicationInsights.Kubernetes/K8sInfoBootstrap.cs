using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal class K8sInfoBootstrap : IK8sInfoBootstrap
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    private readonly AppInsightsForKubernetesOptions _options;

    private readonly object _locker = new object();
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private bool _hasExecuted = false;

    public K8sInfoBootstrap(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<AppInsightsForKubernetesOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Simple mechanism to making sure it is safe to call into this method multiple times.
        lock (_locker)
        {
            if (_hasExecuted)
            {
                return;
            }
            _hasExecuted = true;
        }

        TimeSpan interval = _options.ClusterInfoRefreshInterval;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug($"Starting update K8sEnvironment");

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    IK8sEnvironmentFetcher fetcher = scope.ServiceProvider.GetRequiredService<IK8sEnvironmentFetcher>();
                    await fetcher.UpdateK8sEnvironmentAsync(cancellationToken);
                }

                _logger.LogDebug($"Finished update K8sEnvironment, next iteration will happen at {DateTime.UtcNow.Add(interval)} (UTC) by interval settings of {interval}");

                // TODO: Consider using PeriodicTimer than delay below when move to .NET 6 +
                await Task.Delay(interval, cancellationToken: cancellationToken);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                // In case of any unexpected exception, we shall log it than let it throw.
                _logger.LogError("Unexpected error happened. Telemetry enhancement with Kubernetes info won't happen. Message: {0}", ex.Message);
                _logger.LogTrace("Unexpected error happened. Telemetry enhancement with Kubernetes info won't happen. Details: {0}", ex.ToString());
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }


    // Allows non-hosted service to bootstrap cluster property fetch.
    public void Run(CancellationToken cancellationToken)
    {
        try
        {
            // Fire and forget on purpose to avoid blocking the client code's thread.
            // 
            // Notice: when
            //  1. The cancellation token passed to the user delegate and the one to the Task (2nd parameter) is the same token, and
            //  2. The task is not waited, OperationCancelledException will be handled by Task.Run and it won't propagate.
            // and we don't need to handle the exception.
            _ = Task.Run(async () =>
            {
                await ExecuteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error bootstrapping Kubernetes cluster info. Please file a bug with details: {0}", ex.ToString());
        }
    }
}
