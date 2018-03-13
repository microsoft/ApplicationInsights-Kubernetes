using System;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IK8sEnvironmentFactory
    {
        Task<K8sEnvironment> CreateAsync(TimeSpan timeout);
    }
}