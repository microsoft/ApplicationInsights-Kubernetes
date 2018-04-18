using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Kubernetes;

namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Extnesion method to inject Kubernetes Telemtry Initializer.
    /// </summary>
    public static class ApplicationInsightsExtensions
    {
        public static IServiceCollection EnableKubernetes(
            this IServiceCollection services, 
            TimeSpan? timeout = null,
            IKubernetesServiceCollectionBuilder kubernetesServiceCollectionBuilder = null)
        {
            // Dispatch this on a differnet thread to avoid blocking the main thread.
            // Mainly used with K8s Readness Probe enabled, where communicating with Server will temperory be blocked.
            // TODO: Instead of query the server on the start, we should depend on watch services to provide dynamic realtime data.
            Task.Run(() =>
            {
                KubernetesModule.EnableKubernetes(services, timeout, kubernetesServiceCollectionBuilder);
            });

            return services;
        }
    }
}
