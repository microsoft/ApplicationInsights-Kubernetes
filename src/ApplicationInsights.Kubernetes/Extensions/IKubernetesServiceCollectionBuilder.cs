namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Kubernetes service collection builder.
/// </summary>
internal interface IKubernetesServiceCollectionBuilder
{
    /// <summary>
    /// Injects Application Insights for Kubernetes services into the service collection.
    /// </summary>
    /// <param name="serviceCollection">The collection of service descriptors.</param>
    /// <returns>The collection of services descriptors we injected into.</returns>
    IServiceCollection RegisterServices(IServiceCollection serviceCollection);
}
