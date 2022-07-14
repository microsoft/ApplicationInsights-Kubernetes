#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;

namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

internal class PodInfoManager : IPodInfoManager
{
    private readonly IK8sQueryClientFactory _k8SQueryClientFactory;
    private readonly IKubeHttpClientSettingsProvider _settingsProvider;
    private readonly IEnumerable<IPodNameProvider> _podNameProviders;
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    public PodInfoManager(
        IK8sQueryClientFactory k8SQueryClientFactory,
        IKubeHttpClientSettingsProvider settingsProvider,
        IEnumerable<IPodNameProvider> podNameProviders)
    {
        _k8SQueryClientFactory = k8SQueryClientFactory ?? throw new ArgumentNullException(nameof(k8SQueryClientFactory));
        _settingsProvider = settingsProvider ?? throw new System.ArgumentNullException(nameof(settingsProvider));
        _podNameProviders = podNameProviders ?? throw new System.ArgumentNullException(nameof(podNameProviders));
    }

    /// <inheritdoc />
    public async Task<K8sPod?> GetMyPodAsync(CancellationToken cancellationToken)
    {
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
            K8sPod? targetPod = await GetPodByNameAsync(podName, cancellationToken).ConfigureAwait(false);
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
            K8sPod? targetPod = null;
            using (IK8sQueryClient k8sQueryClient = _k8SQueryClientFactory.Create())
            {
                IEnumerable<K8sPod> allPods = (await k8sQueryClient.GetPodsAsync(cancellationToken).ConfigureAwait(false)).NullAsEmpty();
                targetPod = allPods.FirstOrDefault(pod => pod.GetContainerStatus(myContainerId) != null);
            }

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

    public async Task<K8sPod?> GetPodByNameAsync(string podName, CancellationToken cancellationToken)
    {
        K8sPod? targetPod = null;

        using (IK8sQueryClient k8sQueryClient = _k8SQueryClientFactory.Create())
        {
            targetPod = await k8sQueryClient.GetPodAsync(podName, cancellationToken).ConfigureAwait(false);
        }

        return targetPod;
    }
}
