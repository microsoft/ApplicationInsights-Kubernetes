using k8s;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal class ClusterCheck : IClusterCheck
{
    public bool IsInCluster() => KubernetesClientConfiguration.IsInCluster();
}
