namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;

    using Microsoft.ApplicationInsights.Kubernetes.Entities;
    using Microsoft.Extensions.Logging;

    public class K8sReplicaSetWatcher : K8sWatcher<K8sReplicaSet, K8sReplicaSetMetadata>
    {
        public K8sReplicaSetWatcher(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        protected override string GetRelativePath(string queryNamespace)
        {
            return $@"apis/extensions/v1beta1/namespaces/{queryNamespace}/replicasets?watch=true";
        }
    }
}
