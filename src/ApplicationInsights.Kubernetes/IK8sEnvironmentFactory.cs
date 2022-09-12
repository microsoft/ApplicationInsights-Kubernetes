using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes;

internal interface IK8sEnvironmentFactory
{
    /// <summary>
    /// Creates an instance of an IK8sEnvironment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IK8sEnvironment?> CreateAsync(CancellationToken cancellationToken);
}
