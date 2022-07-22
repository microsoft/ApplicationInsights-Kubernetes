using System;
using System.Collections.Generic;
using System.Linq;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal static class ReplicaSetExtensions
{
    public static V1Deployment? GetMyDeployment(this V1ReplicaSet self, IEnumerable<V1Deployment> scope)
    {
        if (self is null)
        {
            return null;
        }

        V1OwnerReference? ownerReference = self.Metadata.OwnerReferences.FirstOrDefault(o => string.Equals(o.Kind, V1Deployment.KubeKind, StringComparison.Ordinal));
        if (ownerReference is not null)
        {
            V1Deployment? deployment = scope.FirstOrDefault(d => string.Equals(d.Metadata.Uid, ownerReference.Uid, StringComparison.Ordinal));
            return deployment;
        }
        return null;
    }
}
