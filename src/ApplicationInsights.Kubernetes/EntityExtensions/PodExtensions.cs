using System;
using System.Linq;
using System.Collections.Generic;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes;

// Extension methods for Pod
internal static class PodExtensions
{
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
