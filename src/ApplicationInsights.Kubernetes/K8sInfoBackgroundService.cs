using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.ApplicationInsights.Kubernetes;

/// <summary>
/// A bootstrap service to keep updating K8s environment.
/// </summary>
internal class K8sInfoBackgroundService : BackgroundService
{
    private readonly IK8sInfoBootstrap _k8SInfoBootstrap;

    public K8sInfoBackgroundService(IK8sInfoBootstrap k8SInfoBootstrap)
    {
        _k8SInfoBootstrap = k8SInfoBootstrap ?? throw new ArgumentNullException(nameof(k8SInfoBootstrap));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) 
        => _k8SInfoBootstrap.ExecuteAsync(stoppingToken);
}
