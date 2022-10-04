using k8s;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal class ClusterEnvironmentCheck : IClusterEnvironmentCheck
{
    public bool IsInCluster => KubernetesClientConfiguration.IsInCluster();
}
