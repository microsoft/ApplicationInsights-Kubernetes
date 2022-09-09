using System;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A simple implementation to return the Kubernetes information.
/// </summary>
internal class K8sInfoService : IK8sInfoService
{
    private readonly IK8sEnvironmentHolder _k8SEnvironmentHolder;

    public K8sInfoService(IK8sEnvironmentHolder k8SEnvironmentHolder)
    {
        _k8SEnvironmentHolder = k8SEnvironmentHolder ?? throw new ArgumentNullException(nameof(k8SEnvironmentHolder));
    }

    /// <inheritdoc />
    public bool TryGetK8sInfo(out IK8sEnvironment? k8sInfo)
    {
        k8sInfo = null;

        if (_k8SEnvironmentHolder?.K8sEnvironment is null)
        {
            return false;
        }

        if (_k8SEnvironmentHolder.K8sEnvironment is K8sEnvironment envRecord && envRecord is not null)
        {
            k8sInfo = envRecord with { };    // Get a shallow copy of the environment record.
        }

        return k8sInfo is not null;
    }
}
