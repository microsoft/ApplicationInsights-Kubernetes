namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A stub implementation of IK8sInfoService for the customer code to use
/// when running out of the K8s cluster.
/// </summary>
internal class K8sInfoServiceStub : IK8sInfoService
{
    public bool TryGetK8sInfo(out IK8sEnvironment? k8sInfo)
    {
        k8sInfo = null;
        return false;
    }
}

