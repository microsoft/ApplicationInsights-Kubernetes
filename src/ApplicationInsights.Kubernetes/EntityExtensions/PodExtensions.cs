using System;
using System.Linq;
using System.Collections.Generic;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes;

// Extension methods for Pod
internal static class PodExtensions
{
    public static V1ContainerStatus? GetContainerStatus(this V1Pod self, string containerId)
        => string.IsNullOrEmpty(containerId) ? null // Special case when container id is an empty string,
            : self.Status?.ContainerStatuses?.FirstOrDefault(
                // Notice, we are using partial matching because there could be prefix of container ids like: docker://b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4
                status => !string.IsNullOrEmpty(status.ContainerID) && status.ContainerID.IndexOf(containerId, StringComparison.OrdinalIgnoreCase) != -1);

    public static IEnumerable<V1ContainerStatus> GetAllContainerStatus(this V1Pod pod)
    {
        if (pod?.Status is null)
        {
            return Enumerable.Empty<V1ContainerStatus>();
        }
        return pod.Status.ContainerStatuses;
    }

    /// <summary>
    /// Gets the ReplicaSet for the current pod.
    /// </summary>
    /// <param name="self">The target pod.</param>
    /// <param name="scope">List of replicas to search from.</param>
    /// <returns>Returns the replicaSet of the pod. Returns null when the data doesn't exist.</returns>
    public static V1ReplicaSet? GetMyReplicaSet(this V1Pod self, IEnumerable<V1ReplicaSet> scope)
    {
        V1OwnerReference? replicaRef = self.Metadata.OwnerReferences.FirstOrDefault(owner => string.Equals(owner.Kind, V1ReplicaSet.KubeKind, StringComparison.Ordinal));
        if (replicaRef != null)
        {
            V1ReplicaSet? replica = scope?.FirstOrDefault(
                r => string.Equals(r.Metadata.Uid, replicaRef.Uid, StringComparison.OrdinalIgnoreCase));
            return replica;
        }
        return null;
    }
}
