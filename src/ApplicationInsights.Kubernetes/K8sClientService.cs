using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using K8s = k8s.Kubernetes;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A thin wrapper for Kubernetes client to use inside K8s cluster
/// </summary>
internal sealed class K8sClientService : IDisposable, IK8sClientService
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    private bool _isDisposeCalled = false;
    private readonly string _namespace;

    private readonly KubernetesClientConfiguration _configuration;

    private readonly IKubernetes _kubernetesClient;

    private K8sClientService()
    {
        _configuration = KubernetesClientConfiguration.InClusterConfig();
        _namespace = _configuration.Namespace;
        _kubernetesClient = new K8s(_configuration);
    }

    public static K8sClientService Instance { get; } = new K8sClientService();

    public void Dispose()
    {
        if (_isDisposeCalled)
        {
            return;
        }
        _isDisposeCalled = true;

        _kubernetesClient.Dispose();
    }

    public async Task<IEnumerable<V1Pod>> GetPodsAsync(CancellationToken cancellationToken)
    {
        V1PodList? list = await _kubernetesClient.CoreV1.ListNamespacedPodAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
        return list.AsEnumerable();
    }

    public Task<V1Pod?> GetPodByNameAsync(string podName, CancellationToken cancellationToken)
        => _kubernetesClient.CoreV1.ReadNamespacedPodAsync(podName, _namespace, cancellationToken: cancellationToken);

    public async Task<IEnumerable<V1ReplicaSet>> GetReplicaSetsAsync(CancellationToken cancellationToken)
    {
        V1ReplicaSetList? replicaSetList = await _kubernetesClient.AppsV1.ListNamespacedReplicaSetAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
        return replicaSetList.AsEnumerable();
    }

    public async Task<IEnumerable<V1Deployment>> GetDeploymentsAsync(CancellationToken cancellationToken)
    {
        V1DeploymentList? deploymentList = await _kubernetesClient.AppsV1.ListNamespacedDeploymentAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
        return deploymentList.AsEnumerable();
    }

    public async Task<IEnumerable<V1Node>> GetNodesAsync(bool ignoreForbiddenException, CancellationToken cancellationToken)
    {
        try
        {
            V1NodeList? nodeList = await _kubernetesClient.CoreV1.ListNodeAsync().ConfigureAwait(false);
            return nodeList.AsEnumerable();
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Forbidden)
        {
            // Choose to ignore forbidden exception
            if (ignoreForbiddenException)
            {
                _logger.LogDebug(ex.Message);
                _logger.LogTrace(ex.ToString());
                return Enumerable.Empty<V1Node>();
            }

            // Otherwise
            throw;
        }
    }
}
