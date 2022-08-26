namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// Provides a single point of access for K8sEnvironment object.
/// </summary>
internal sealed class K8sEnvironmentHolder
{
    private K8sEnvironmentHolder() { }

    public static K8sEnvironmentHolder Instance { get; } = new K8sEnvironmentHolder();

    /// <summary>
    /// Gets or sets the kubernetes environment object.
    /// Returns null when the environment is info is not ready for consuming.
    /// </summary>
    public IK8sEnvironment? K8sEnvironment { get; internal set; }
}
