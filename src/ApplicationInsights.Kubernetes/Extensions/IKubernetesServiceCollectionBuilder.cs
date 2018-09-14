using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Kubernetes service collection builder.
    /// </summary>
    public interface IKubernetesServiceCollectionBuilder
    {
        /// <summary>
        /// Injects Application Insights for Kubernetes services into the service collection.
        /// </summary>
        /// <param name="serviceCollection">The collection of service descriptors.</param>
        /// <param name="timeout">Maximum time to wait for spinning up the container.</param>
        /// <returns>The collection of services descripters we injected into.</returns>
        IServiceCollection InjectServices(IServiceCollection serviceCollection, TimeSpan timeout);
    }
}