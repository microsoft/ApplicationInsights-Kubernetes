namespace Microsoft.ApplicationInsights.Kubernetes;

internal interface IClusterCheck
{
    bool IsInCluster();
}
