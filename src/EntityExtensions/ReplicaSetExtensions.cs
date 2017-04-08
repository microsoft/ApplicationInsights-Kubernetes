namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity;

    internal static class ReplicaSetExtensions
    {
        public static K8sDeployment GetMyDeployment(this ReplicaSet self, IEnumerable<K8sDeployment> scope)
        {
            IDictionary<string, string> replicaLabels = self.Metadata.Labels;

            foreach (K8sDeployment deployment in scope)
            {
                IDictionary<string, string> matchRule = deployment.Spec.Selector.MatchLabels;
                if (matchRule.Intersect(replicaLabels).Count() == matchRule.Count)
                {
                    // All labels are matched
                    return deployment;
                }
            }
            return null;
        }
    }
}
