namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// An instance contains Kubernetes environment information.
/// </summary>
public interface IK8sEnvironment
{
    /// <summary>
    /// Gets the id of the Container. Null when the value is not available.
    /// </summary>
    string? ContainerID { get; }

    /// <summary>
    /// Gets the name of the Container. Null when the value is not available.
    /// </summary>
    string? ContainerName { get; }

    /// <summary>
    /// Name of the image specified in deployment spec.
    /// </summary>
    string? ImageName { get; }

    /// <summary>
    /// Gets the unique id of the Deployment. Null when the value is not available.
    /// </summary>
    string? DeploymentUid { get; }

    /// <summary>
    /// Gets the name of the Deployment. Null when the value is not available.
    /// </summary>
    string? DeploymentName { get; }

    /// <summary>
    /// Gets the name of the Node. Null when the value is not available.
    /// </summary>
    string? NodeName { get; }

    /// <summary>
    /// Gets the unique id of the Node. Null when the value is not available.
    /// </summary>
    string? NodeUid { get; }

    /// <summary>
    /// Gets the id of the Pod.
    /// </summary>
    string PodID { get; }

    /// <summary>
    /// Gets the labels of the pod in form of a string. The key values are separated by colons(':') and the labels are separated by commas(',').
    /// </summary>
    /// <value></value>
    string? PodLabels { get; }

    /// <summary>
    /// Gets the Namespace. Null when the value is not available.
    /// </summary>
    string? PodNamespace { get; }

    /// <summary>
    /// Gets the name of the Pod.
    /// </summary>
    string PodName { get; }

    /// <summary>
    /// Gets the unique id of the ReplicaSet. Null when the value is not available.
    /// </summary>
    /// <value></value>
    string? ReplicaSetUid { get; }

    /// <summary>
    /// Gets the name of the ReplicaSet. Null when the value is not available.
    /// </summary>
    /// <value></value>
    string? ReplicaSetName { get; }
}
