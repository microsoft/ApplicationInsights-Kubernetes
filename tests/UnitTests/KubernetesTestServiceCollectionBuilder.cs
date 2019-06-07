using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubernetesTestServiceCollectionBuilder : KubernetesServiceCollectionBuilder
    {
        public KubernetesTestServiceCollectionBuilder() : base(() => true)
        {
        }

        protected override void InjectChangableServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpDebuggingClientSettings>();
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sDebuggingEnvironmentFactory>();
        }
    }
}
