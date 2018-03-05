using System;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Kubernetes.Stubs
{
    internal class K8sEnvironmentStubFactory : IK8sEnvironmentFactory
    {
        public Task<K8sEnvironment> CreateAsync(TimeSpan timeout)
        {
            return Task.FromResult(new K8sEnvironment()
            {
                ContainerID = KubeHttpClientSettingsStub.FakeContainerId
            });
        }
    }
}
