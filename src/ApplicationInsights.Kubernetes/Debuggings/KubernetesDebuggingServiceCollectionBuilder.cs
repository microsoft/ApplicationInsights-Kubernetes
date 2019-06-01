using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        public KubernetesDebuggingServiceCollectionBuilder() : base(() => true) { }

        /// <summary>
        /// Injects the Application Insights for Kubernetes debugging services.
        /// </summary>
        /// <param name="serviceCollection">The service collection to inject the debugging services into.</param>
        protected override void InjectChangableServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpDebuggingClientSettings>();
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sDebuggingEnvironmentFactory>();
        }
    }
}
