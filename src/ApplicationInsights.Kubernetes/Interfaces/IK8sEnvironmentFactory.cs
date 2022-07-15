using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IK8sEnvironmentFactory
    {
        Task<IK8sEnvironment> CreateAsync(DateTime timeoutAt, CancellationToken cancellationToken);
    }
}
