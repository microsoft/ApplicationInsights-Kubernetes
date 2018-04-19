using System;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IK8sEnvironmentFactory
    {
        Task<IK8sEnvironment> CreateAsync(DateTime timeoutAt);
    }
}