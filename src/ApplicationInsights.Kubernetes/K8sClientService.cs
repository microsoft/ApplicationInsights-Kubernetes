using System;
using k8s;
using K8s = k8s.Kubernetes;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A thin wrapper for Kubernetes client to use inside K8s cluster
/// </summary>
internal sealed class K8sClientService : IDisposable, IK8sClientService
{
    private bool _isDisposeCalled = false;

    private readonly KubernetesClientConfiguration _configuration;

    private readonly IKubernetes _kubernetesClient;

    private K8sClientService()
    {
        _configuration = KubernetesClientConfiguration.InClusterConfig();
        _kubernetesClient = new K8s(_configuration);
    }

    public static K8sClientService Instance { get; } = new K8sClientService();

    public IKubernetes Client => _kubernetesClient;

    public string Namespace => _configuration.Namespace;

    public bool InCluster { get; }

    public void Dispose()
    {
        if (_isDisposeCalled)
        {
            return;
        }
        _isDisposeCalled = true;

        _kubernetesClient.Dispose();
    }
}
