using k8s;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal interface IK8sClientService
{
    /// <summary>
    /// Gets a Kubernetes Client
    /// </summary>
    IKubernetes Client { get; }

    /// <summary>
    /// Gets the Kubernetes Namespace of the application.
    /// </summary>
    string Namespace { get; }
}
