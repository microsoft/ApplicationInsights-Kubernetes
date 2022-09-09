namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A service to return the Kubernetes information.
/// </summary>
public interface IK8sInfo
{
    /// <summary>
    /// Tries to get the kubernetes info.
    /// </summary>
    /// <returns>Returns true when the fetch succeeded. False otherwise.</returns>
    bool TryGetK8sInfo(out IK8sEnvironment? k8sInfo);
}
