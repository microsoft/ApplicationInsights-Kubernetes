namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A service to check if the code is running in a Kubernetes cluster or not.
/// </summary>
public interface IClusterEnvironmentCheck
{
    /// <summary>
    /// A property to indicate whether the current process is running inside a Kubernetes environment or not.
    /// </summary>
    bool IsInCluster { get; }
}
