using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal class K8sEnvironmentFetcher : IK8sEnvironmentFetcher
{
    private readonly IK8sEnvironmentHolder _k8sEnvironmentHolder;
    private readonly IK8sEnvironmentFactory _k8SEnvironmentFactory;
    private readonly ILogger _logger;
    private readonly AppInsightsForKubernetesOptions _options;

    public K8sEnvironmentFetcher(
        IK8sEnvironmentHolder environmentHolder,
        IK8sEnvironmentFactory k8SEnvironmentFactory,
        IOptions<AppInsightsForKubernetesOptions> options,
        ILogger<K8sEnvironmentFetcher> logger)
    {
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new System.ArgumentNullException(nameof(options));

        _k8sEnvironmentHolder = environmentHolder ?? throw new System.ArgumentNullException(nameof(environmentHolder));
        _k8SEnvironmentFactory = k8SEnvironmentFactory ?? throw new System.ArgumentNullException(nameof(k8SEnvironmentFactory));
    }

    public async Task UpdateK8sEnvironmentAsync(CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = new CancellationTokenSource(_options.InitializationTimeout);
        CancellationToken timeoutToken = timeoutSource.Token;

        try
        {
            using (CancellationTokenSource linkedTokeSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken))
            {
                // Build a new environment instance to replace the last one.
                IK8sEnvironment? environment = await _k8SEnvironmentFactory.CreateAsync(cancellationToken: linkedTokeSource.Token).ConfigureAwait(false);
                _k8sEnvironmentHolder.K8sEnvironment = environment;
            }
        }
        catch (OperationCanceledException)
        {
            if (timeoutToken.IsCancellationRequested)
            {
                _logger.LogError("Querying Kubernetes cluster info timed out.");
                _k8sEnvironmentHolder.K8sEnvironment = null;
            }
            else
            {
                _logger.LogInformation("Operation cancelled.");
            }
        }
    }
}
