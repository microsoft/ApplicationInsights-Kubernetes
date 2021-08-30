using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Application Insights for Kubernetes service collection builder specifically for debugging purpose.
    /// </summary>
    public sealed class KubernetesDebuggingServiceCollectionBuilder : KubernetesServiceCollectionBuilder
    {
        /// <summary>
        /// Constructor for <see cref="KubernetesDebuggingServiceCollectionBuilder"/>.
        /// </summary>
        public KubernetesDebuggingServiceCollectionBuilder(IOptions<AppInsightsForKubernetesOptions> options)
        : base(isRunningInKubernetes: () => true) { }

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
