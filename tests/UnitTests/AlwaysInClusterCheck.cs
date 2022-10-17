namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

public sealed class AlwaysInClusterCheck : IClusterEnvironmentCheck
{
    private AlwaysInClusterCheck() { }
    public static AlwaysInClusterCheck Instance { get; } = new AlwaysInClusterCheck();

    public bool IsInCluster => true;
}
