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
internal class K8sInfoBackgroundService : BackgroundService
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    private readonly IServiceProvider _serviceProvider;
    private readonly K8sEnvironmentHolder _k8SEnvironmentHolder;
    private readonly AppInsightsForKubernetesOptions _options;

    public K8sInfoBackgroundService(
        IServiceProvider serviceProvider,
        K8sEnvironmentHolder k8SEnvironmentHolder,
        IOptions<AppInsightsForKubernetesOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _k8SEnvironmentHolder = k8SEnvironmentHolder ?? throw new ArgumentNullException(nameof(k8SEnvironmentHolder));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                _logger.LogDebug($"Starting {nameof(UpdateK8sEnvironmentAsync)}");
                await UpdateK8sEnvironmentAsync(stoppingToken).ConfigureAwait(false);
                TimeSpan interval = _options.ClusterInfoRefreshInterval;
                _logger.LogDebug($"Finished {nameof(UpdateK8sEnvironmentAsync)}, delay for {interval}");
                await Task.Delay(interval);
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
        await using (AsyncServiceScope scope = _serviceProvider.CreateAsyncScope())
        {
            // Get scoped service ready
            IServiceProvider provider = scope.ServiceProvider;
            IK8sEnvironmentFactory factory = provider.GetRequiredService<IK8sEnvironmentFactory>();

            // Prepare cancellation token with timeout.
            using CancellationTokenSource timeoutSource = new CancellationTokenSource(_options.InitializationTimeout);
            CancellationToken timeoutToken = timeoutSource.Token;
            
            try
            {
                using (CancellationTokenSource linkedTokeSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken))
                {

                    IK8sEnvironment? environment = await factory.CreateAsync(cancellationToken: linkedTokeSource.Token).ConfigureAwait(false);
                    _k8SEnvironmentHolder.K8sEnvironment = environment;
                }
            }
            catch (OperationCanceledException)
            {
                if (timeoutToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Querying Kubernetes cluster info timed out.");
                    _k8SEnvironmentHolder.K8sEnvironment = null;
                    _k8SEnvironmentHolder.IsQueryTimeout = true;
                }
                else
                {
                    _logger.LogInformation("Operation cancelled.");
                }
            }
        }
    }
}
