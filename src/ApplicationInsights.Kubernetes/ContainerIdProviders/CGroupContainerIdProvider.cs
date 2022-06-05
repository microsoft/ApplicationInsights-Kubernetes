#nullable enable

using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders
{
    /// <summary>
    /// Gets the current container id by using CGroup
    /// </summary>
    internal class CGroupContainerIdProvider : FileBasedContainerIdProvider
    {
        private const string CGroupPath = "/proc/self/cgroup";
        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        public CGroupContainerIdProvider(
            CGroupV1Matcher lineMatcher,
            IStreamLineReader streamLineReader) : 
            base(lineMatcher, streamLineReader, CGroupPath, providerName: default)
        {
        }
    }
}
