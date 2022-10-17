using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

public class ServiceProviderLifetimeValidationTests
{
    [Fact]
    public void ContainerIdHolderShallNotCaptureScopedServices()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddApplicationInsightsKubernetesEnricher(clusterCheck: AlwaysInClusterCheck.Instance);

        using (ServiceProvider sp = services.BuildServiceProvider(validateScopes: true))
        {
            // Container id holder shall be constructed without scope validation error.
            IContainerIdHolder containerIdHolder = sp.GetRequiredService<IContainerIdHolder>();
            // Pass when there has no exception at this point.
        }
    }
}
