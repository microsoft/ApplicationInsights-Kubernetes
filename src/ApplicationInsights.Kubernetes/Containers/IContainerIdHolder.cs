using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal interface IContainerIdHolder
{
    string? ContainerId { get; }

    bool TryBackFillContainerId(V1Pod pod, out V1ContainerStatus? containerStatus);
}
