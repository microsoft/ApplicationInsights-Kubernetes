using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A bootstrap service to keep updating K8s environment.
/// </summary>
internal class K8sInfoBackgroundService : BackgroundService, IK8sInfoBootstrap
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly AppInsightsForKubernetesOptions _options;

    private readonly object _locker = new object();
    private bool _hasExecuted = false;

    // Notice: This is a background service, the service lifetime will be singleton.
    // Do NOT inject services in scope of Scoped or Transient. Use the injected service provider instead.
    public K8sInfoBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<AppInsightsForKubernetesOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    // Allows non-hosted service to bootstrap cluster info fetch.
    void IK8sInfoBootstrap.Run()
    {
        try
        {
            // Fire and forget on purpose to avoid blocking the client code's thread.
            _ = Task.Run(async () =>
            {
                await ExecuteAsync(stoppingToken: default).ConfigureAwait(false);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error bootstrapping Kubernetes cluster info. Please file a bug with details: {0}", ex.ToString());
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug($"Starting update K8sEnvironment");

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    IK8sEnvironmentFetcher fetcher = scope.ServiceProvider.GetRequiredService<IK8sEnvironmentFetcher>();
                    await fetcher.UpdateK8sEnvironmentAsync(stoppingToken);
                }

                _logger.LogDebug($"Finished update K8sEnvironment, next iteration will happen at {DateTime.UtcNow.Add(interval)} (UTC) by interval settings of {interval}");

                // TODO: Consider using PeriodicTimer than delay below when move to .NET 6 +
                await Task.Delay(interval, cancellationToken: stoppingToken);
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
}
