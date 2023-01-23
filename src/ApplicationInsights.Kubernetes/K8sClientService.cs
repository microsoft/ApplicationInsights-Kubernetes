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
#if NETSTANDARD2_1
        V1PodList? list = await _kubernetesClient.ListNamespacedPodAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
#else
        V1PodList? list = await _kubernetesClient.CoreV1.ListNamespacedPodAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
#endif
        return list.AsEnumerable();
    }

    public Task<V1Pod?> GetPodByNameAsync(string podName, CancellationToken cancellationToken)
#if NETSTANDARD2_1
            => _kubernetesClient.ReadNamespacedPodAsync(podName, _namespace, cancellationToken: cancellationToken);
#else
            => _kubernetesClient.CoreV1.ReadNamespacedPodAsync(podName, _namespace, cancellationToken: cancellationToken);
#endif

    public async Task<IEnumerable<V1ReplicaSet>> GetReplicaSetsAsync(CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1
        V1ReplicaSetList? replicaSetList = await _kubernetesClient.ListNamespacedReplicaSetAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
#else
        V1ReplicaSetList? replicaSetList = await _kubernetesClient.AppsV1.ListNamespacedReplicaSetAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
#endif
        return replicaSetList.AsEnumerable();
    }

    public async Task<IEnumerable<V1Deployment>> GetDeploymentsAsync(CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1
        V1DeploymentList? deploymentList = await _kubernetesClient.ListNamespacedDeploymentAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
#else
        V1DeploymentList? deploymentList = await _kubernetesClient.AppsV1.ListNamespacedDeploymentAsync(_namespace, cancellationToken: cancellationToken).ConfigureAwait(false);
#endif
        return deploymentList.AsEnumerable();
    }

    public async Task<IEnumerable<V1Node>> GetNodesAsync(bool ignoreForbiddenException, CancellationToken cancellationToken)
    {
        try
        {
#if NETSTANDARD2_1
            V1NodeList? nodeList = await _kubernetesClient.ListNodeAsync().ConfigureAwait(false);
#else
            V1NodeList? nodeList = await _kubernetesClient.CoreV1.ListNodeAsync().ConfigureAwait(false);
#endif
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
