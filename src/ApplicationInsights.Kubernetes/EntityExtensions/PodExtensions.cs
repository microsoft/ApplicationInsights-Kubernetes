#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Kubernetes.Entities;
    using Microsoft.ApplicationInsights.Kubernetes.Utilities;

    // Extension methods for Pod
    internal static class PodExtensions
    {
        public static ContainerStatus? GetContainerStatus(this K8sPod self, string containerId)
            => string.IsNullOrEmpty(containerId) ? null // Special case when container id is an empty string,
                : self?.Status?.ContainerStatuses?.FirstOrDefault(status => string.Equals(status.ContainerID, containerId, StringComparison.Ordinal));

        public static IEnumerable<ContainerStatus> GetAllContainerStatus(this K8sPod pod)
        {
            if (pod?.Status is null)
            {
                return Enumerable.Empty<ContainerStatus>();
            }
            return pod.Status.ContainerStatuses.NullAsEmpty();
        }

        /// <summary>
        /// Gets the ReplicaSet for the current pod.
        /// </summary>
        /// <param name="self">The target pod.</param>
        /// <param name="scope">List of replicas to search from.</param>
        /// <returns>Returns the replicaSet of the pod. Returns null when the data doesn't exist.</returns>
        public static K8sReplicaSet? GetMyReplicaSet(this K8sPod self, IEnumerable<K8sReplicaSet> scope)
        {
            OwnerReference? replicaRef = self.Metadata?.OwnerReferences?.FirstOrDefault(owner => owner.GetKind() != null && owner.GetKind() == typeof(K8sReplicaSet));
            if (replicaRef != null)
            {
                K8sReplicaSet? replica = scope?.FirstOrDefault(
                    r => r.Metadata != null &&
                    r.Metadata.Uid != null &&
                    r.Metadata.Uid.Equals(replicaRef.Uid, StringComparison.OrdinalIgnoreCase));
                return replica;
            }
            return null;
        }
    }
}
