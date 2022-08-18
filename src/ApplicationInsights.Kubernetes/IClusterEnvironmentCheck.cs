namespace Microsoft.ApplicationInsights.Kubernetes;

internal interface IClusterEnvironmentCheck
{
    bool IsInCluster { get; }
}
