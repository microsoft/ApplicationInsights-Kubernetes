using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    public sealed class KubernetesDebuggingServiceCollectionBuilder : KubernetesServiceCollectionBuilder
    {
        public KubernetesDebuggingServiceCollectionBuilder(ILogger<KubernetesDebuggingServiceCollectionBuilder> logger) : base(new DebuggingK8sDetector(), logger) { }

        protected override void InjectChangableServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpDebuggingClientSettings>();
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sDebuggingEnvironmentFactory>();
        }
    }
}
