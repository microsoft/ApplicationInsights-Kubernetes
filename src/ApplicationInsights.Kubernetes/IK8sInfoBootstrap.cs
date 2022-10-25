using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A service to fetch kubernetes information for consumption.
/// The intention is for the client to have a handle to start getting Kubernetes info to be consumed by the <see cref="IK8sInfoService" />.
/// Remark: This is supposed to only be used in Console Application. Do NOT use this in ASP.NET or Worker, where the hosted service exists.
/// </summary>
internal interface IK8sInfoBootstrap
{
    /// <summary>
    /// Bootstrap the fetch of Kubernetes information.
    /// Run() is should NOT block the current thread.
    /// </summary>
    void Run(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bootstrap the fetch of Kubernetes information.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
}
