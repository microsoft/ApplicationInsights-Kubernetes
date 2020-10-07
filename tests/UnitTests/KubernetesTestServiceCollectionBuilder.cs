using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubernetesTestServiceCollectionBuilder : KubernetesServiceCollectionBuilder
    {
        public KubernetesTestServiceCollectionBuilder()
            : base(isRunningInKubernetes: () => true, Options.Create(new AppInsightsForKubernetesOptions()))
        {
        }

        protected override void InjectChangableServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpDebuggingClientSettings>();
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sDebuggingEnvironmentFactory>();
        }
    }
}
