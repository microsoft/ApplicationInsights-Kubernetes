#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Entities;

namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

internal class PodInfoManager : IPodInfoManager
{
    private readonly IK8sQueryClient _k8SQueryClient;
    private readonly IKubeHttpClientSettingsProvider _settingsProvider;
    private readonly IEnumerable<IPodNameProvider> _podNameProviders;
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    public PodInfoManager(
        IK8sQueryClient k8SQueryClient,
        IKubeHttpClientSettingsProvider settingsProvider,
        IEnumerable<IPodNameProvider> podNameProviders)
    {
        _k8SQueryClient = k8SQueryClient ?? throw new System.ArgumentNullException(nameof(k8SQueryClient));
        _settingsProvider = settingsProvider ?? throw new System.ArgumentNullException(nameof(settingsProvider));
        _podNameProviders = podNameProviders ?? throw new System.ArgumentNullException(nameof(podNameProviders));
    }

    /// <inheritdoc />
    public async Task<K8sPod?> GetMyPodAsync(CancellationToken cancellationToken)
    {
        IEnumerable<K8sPod> allPods = await _k8SQueryClient.GetPodsAsync().ConfigureAwait(false);

        string podName = string.Empty;
        foreach (IPodNameProvider podNameProvider in _podNameProviders)
        {
            if (podNameProvider.TryGetPodName(out podName))
            {
                // Got pod name successfully.
                break;
            }
        }
        _logger.LogDebug($"Pod name by providers: {podName}");

        // Find pod by name
        if (!string.IsNullOrEmpty(podName))
        {
            K8sPod? targetPod = allPods.FirstOrDefault(p => string.Equals(p.Metadata?.Name, podName, StringComparison.Ordinal));
            if (targetPod is not null)
            {
                _logger.LogInformation($"Found pod by name providers: {targetPod.Metadata?.Name}");
                return targetPod;
            }
        }
        else
        {
            _logger.LogTrace("Pod name is not fetched by providers.");
        }

        // Find pod by container id:
        string myContainerId = _settingsProvider.ContainerId;
        _logger.LogDebug($"Looking for pod name by container id: {myContainerId}");

        if (!string.IsNullOrEmpty(myContainerId))
        {
            K8sPod? targetPod = allPods.FirstOrDefault(pod => pod.GetContainerStatus(myContainerId) != null);

            if (targetPod is not null)
            {
                _logger.LogInformation($"Found pod by container id. Pod name: {targetPod.Metadata?.Name}");
                return targetPod;
            }
        }
        else
        {
            _logger.LogTrace("Container id is not available for use to locate pod.");
        }

        _logger.LogError("Pod name can't be determined.");
        return null;
    }
}
