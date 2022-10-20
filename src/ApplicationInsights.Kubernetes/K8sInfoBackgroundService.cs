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
    private readonly IK8sEnvironmentHolder _k8SEnvironmentHolder;
    private readonly AppInsightsForKubernetesOptions _options;

    // Notice: This is a background service, the service lifetime will be singleton.
    // Do NOT inject services in scope of Scoped or Transient. Use the injected service provider instead.
    public K8sInfoBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IK8sEnvironmentHolder k8SEnvironmentHolder,
        IOptions<AppInsightsForKubernetesOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _k8SEnvironmentHolder = k8SEnvironmentHolder ?? throw new ArgumentNullException(nameof(k8SEnvironmentHolder));
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
                using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                {
                    await ExecuteAsync(cancellationTokenSource.Token).ConfigureAwait(false);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error bootstrapping Kubernetes cluster info. Please file a bug with details: {0}", ex.ToString());
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan interval = _options.ClusterInfoRefreshInterval;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug($"Starting {nameof(UpdateK8sEnvironmentAsync)}");
                await UpdateK8sEnvironmentAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
                _logger.LogDebug($"Finished {nameof(UpdateK8sEnvironmentAsync)}, next iteration will happen at {DateTime.UtcNow.Add(interval)} (UTC) by interval settings of {interval}");

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

    private async Task UpdateK8sEnvironmentAsync(CancellationToken cancellationToken)
    {
        await using (AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
        {
            // Get scoped services ready
            IServiceProvider provider = scope.ServiceProvider;
            IK8sEnvironmentFactory factory = provider.GetRequiredService<IK8sEnvironmentFactory>();

            // Prepare cancellation token with timeout.
            using CancellationTokenSource timeoutSource = new CancellationTokenSource(_options.InitializationTimeout);
            CancellationToken timeoutToken = timeoutSource.Token;

            try
            {
                using (CancellationTokenSource linkedTokeSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken))
                {
                    // Build a new environment instance to replace the last one.
                    IK8sEnvironment? environment = await factory.CreateAsync(cancellationToken: linkedTokeSource.Token).ConfigureAwait(false);
                    _k8SEnvironmentHolder.K8sEnvironment = environment;
                }
            }
            catch (OperationCanceledException)
            {
                if (timeoutToken.IsCancellationRequested)
                {
                    _logger.LogError("Querying Kubernetes cluster info timed out.");
                    _k8SEnvironmentHolder.K8sEnvironment = null;
                }
                else
                {
                    _logger.LogInformation("Operation cancelled.");
                }
            }
        }
    }
}
