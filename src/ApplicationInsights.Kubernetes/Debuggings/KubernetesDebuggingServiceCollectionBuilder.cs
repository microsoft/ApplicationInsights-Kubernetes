using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    public sealed class KubernetesDebuggingServiceCollectionBuilder : KubernetesServiceCollectionBuilder
    {
        #region Singleton
        private KubernetesDebuggingServiceCollectionBuilder() { }
        private static KubernetesDebuggingServiceCollectionBuilder _instance = new KubernetesDebuggingServiceCollectionBuilder();

        [Obsolete("This instance is used only for debugging. Never use this in production!", false)]
        public static KubernetesDebuggingServiceCollectionBuilder Instance => _instance;
        #endregion

        protected override void InjectChangableServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKubeHttpClientSettingsProvider, KubeHttpDebuggingClientSettings>();
            serviceCollection.AddSingleton<IK8sEnvironmentFactory, K8sDebuggingEnvironmentFactory>();
        }
    }
}
