namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    using System.Linq;
    using Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity;

    // Extension methods for Pod
    public static class PodExtensions
    {
        public static ContainerStatus GetContainerStatus(this Pod self, string containerId)
        {
            ContainerStatus result = self.Status.ContainerStatuses?.FirstOrDefault(
                cs => !string.IsNullOrEmpty(cs.ContainerID) && cs.ContainerID.EndsWith(containerId, System.StringComparison.Ordinal));
            return result;
        }
    }
}
