using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using k8s.Autorest;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Pods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal class K8sEnvironmentFactory : IK8sEnvironmentFactory
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    private readonly IPodInfoManager _podInfoManager;
    private readonly IContainerStatusManager _containerStatusManager;
    private readonly IK8sClientService _k8sClient;
    private readonly AppInsightsForKubernetesOptions _options;

    public K8sEnvironmentFactory(
        IPodInfoManager podInfoManager,
        IContainerStatusManager containerStatusManager,
        IK8sClientService k8sClient,
        IOptions<AppInsightsForKubernetesOptions> options)
    {
        _podInfoManager = podInfoManager ?? throw new ArgumentNullException(nameof(podInfoManager));
        _containerStatusManager = containerStatusManager ?? throw new ArgumentNullException(nameof(containerStatusManager));
        _k8sClient = k8sClient ?? throw new ArgumentNullException(nameof(k8sClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Async factory method to build the instance of a K8sEnvironment.
    /// </summary>
    /// <returns></returns>
    public async Task<IK8sEnvironment?> CreateAsync(CancellationToken cancellationToken)
    {
        try
        {
            // When my pod become available and it's status become ready, we recognize the container is ready.
            V1Pod myPod = await _podInfoManager.WaitUntilMyPodReadyAsync(cancellationToken).ConfigureAwait(false);

            // Wait until the container is ready.
            V1ContainerStatus? containerStatus = await _containerStatusManager.GetMyContainerStatusAsync(cancellationToken).ConfigureAwait(false);

            // Fetch replica set info
            IEnumerable<V1ReplicaSet> allReplicaSet = await _k8sClient.GetReplicaSetsAsync(cancellationToken).ConfigureAwait(false);
            V1ReplicaSet? replicaSet = myPod.GetMyReplicaSet(allReplicaSet);

            // Fetch deployment info
            IEnumerable<V1Deployment> allDeployment = await _k8sClient.GetDeploymentsAsync(cancellationToken).ConfigureAwait(false);
            V1Deployment? deployment = replicaSet?.GetMyDeployment(allDeployment);

            // Fetch node info
            V1Node? node = null;
            if (!_options.ExcludeNodeInformation)
            {
                string nodeName = myPod.Spec.NodeName;
                IEnumerable<V1Node> allNodes = await _k8sClient.GetNodesAsync(ignoreForbiddenException: true, cancellationToken).ConfigureAwait(false);
                node = allNodes.FirstOrDefault(n => string.Equals(n.Metadata.Name, nodeName, StringComparison.Ordinal));   
            }
            
            K8sEnvironment k8SEnvironment = new K8sEnvironment(containerStatus, myPod, replicaSet, deployment, node);
            _logger.LogDebug(JsonSerializer.Serialize<K8sEnvironment>(k8SEnvironment).EscapeForLoggingMessage());
            return k8SEnvironment;
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Forbidden)
        {
            HandleUnauthorizedAccess(ex);
            return null;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            _logger.LogCritical(ex.ToString());
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    private void HandleUnauthorizedAccess(HttpOperationException exception)
    {
        Debug.Assert(exception.Response.StatusCode == HttpStatusCode.Forbidden, "Only handle Forbidden!");
        _logger.LogError(
            "Unauthorized. Are you missing cluster role assignment? Refer to https://aka.ms/ai-k8s-rbac for more details. Message: {0}.",
            exception.Message);
        _logger.LogDebug(exception.ToString());
    }
}
