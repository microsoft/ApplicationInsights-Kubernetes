using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Stubs;

namespace Microsoft.Extensions.DependencyInjection
{
    public sealed class KubernetesDebuggingServiceCollectionBuilder : KubernetesServiceCollectionBuilder
    {
        #region Singleton
        private KubernetesDebuggingServiceCollectionBuilder() { }
        private static KubernetesDebuggingServiceCollectionBuilder _instance = new KubernetesDebuggingServiceCollectionBuilder();
        public static KubernetesDebuggingServiceCollectionBuilder Instance => _instance;
        #endregion

        protected override void InjectChangableServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpClientSettingsStub>();
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sEnvironmentStubFactory>();
        }
    }
}
