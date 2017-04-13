
namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using Microsoft.ApplicationInsights.Kubernetes.Entities;
    internal static class OwnerReferenceExtensions
    {
        public static Type GetKind(this OwnerReference self)
        {
            switch (self.Kind)
            {
                case "ReplicaSet":
                    return typeof(K8sReplicaSet);
                default:
                    return null;
            }
        }
    }
}
