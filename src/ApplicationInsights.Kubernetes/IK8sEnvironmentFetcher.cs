using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    /// <summary>
    /// A service to fetch the K8s cluster properties from the cluster, and put it onto <see cref="IK8sEnvironmentHolder" />.
    /// </summary>
    internal interface IK8sEnvironmentFetcher
    {
        /// <summary>
        /// Update K8s cluster properties once.
        /// </summary>
        Task UpdateK8sEnvironmentAsync(CancellationToken cancellationToken);
    }
}
