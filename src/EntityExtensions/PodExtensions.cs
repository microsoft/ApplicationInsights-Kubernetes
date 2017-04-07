namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity;

    // Extension methods for Pod
    internal static class PodExtensions
    {
        public static ContainerStatus GetContainerStatus(this Pod self, string containerId)
        {
            ContainerStatus result = self.Status.ContainerStatuses?.FirstOrDefault(
                cs => !string.IsNullOrEmpty(cs.ContainerID) && cs.ContainerID.EndsWith(containerId, StringComparison.Ordinal));
            return result;
        }

        /// <summary>
        /// Get the ReplicaSet for the current pod.
        /// </summary>
        /// <param name="self">The target pod.</param>
        /// <param name="scope">List of replicas to search from.</param>
        /// <returns>Returns the replicaSet of the pod. Returns null when the data doens't exist.</returns>
        public static ReplicaSet GetMyReplicaSet(this Pod self, IEnumerable<ReplicaSet> scope)
        {
            OwnerReference replicaRef = self.Metadata?.OwnerReferences?.FirstOrDefault(owner => owner.GetKind() != null && owner.GetKind() == typeof(ReplicaSet));
            if (replicaRef != null)
            {
                ReplicaSet replica = scope?.FirstOrDefault(
                    r => r.Metadata != null &&
                    r.Metadata.Uid != null &&
                    r.Metadata.Uid.Equals(replicaRef.Uid, StringComparison.OrdinalIgnoreCase));
                return replica;
            }
            return null;
        }
    }
}
