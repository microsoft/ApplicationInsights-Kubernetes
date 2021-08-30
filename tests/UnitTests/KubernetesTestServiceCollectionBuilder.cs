using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubernetesTestServiceCollectionBuilder : KubernetesServiceCollectionBuilder
    {
        public KubernetesTestServiceCollectionBuilder()
            : base(isRunningInKubernetes: () => true)
        {
        }

        /// <summary>
        /// Registers setttings provider for querying K8s proxy.
        /// </summary>
        /// <param name="serviceCollection"></param>
        protected override void RegisterSettingsProvider(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpDebuggingClientSettings>();
        }

        /// <summary>
        /// Registers K8s environment factory.
        /// </summary>
        protected override void RegisterK8sEnvironmentFactory(IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sDebuggingEnvironmentFactory>();
    }
}
