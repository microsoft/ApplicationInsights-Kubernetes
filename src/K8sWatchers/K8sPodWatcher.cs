namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;

    using Microsoft.ApplicationInsights.Kubernetes.Entities;
    using Microsoft.Extensions.Logging;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;
    public class K8sPodWatcher : K8sWatcher<K8sPod, K8sPodMetadata>
    {
        public K8sPodWatcher(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        protected override string GetRelativePath(string queryNamespace)
        {
            return Invariant($"api/v1/namespaces/{queryNamespace}/pods?watch=true");
        }
    }
}
