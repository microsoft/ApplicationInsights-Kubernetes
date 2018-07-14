using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IKubernetesServiceCollectionBuilder
    {
        IServiceCollection InjectServices(IServiceCollection serviceCollection, TimeSpan timeout);
    }
}