using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    static class TestUtils
    {
        public static ILogger<T> GetLogger<T>()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            return serviceCollection.BuildServiceProvider().GetService<ILogger<T>>();
        }
    }
}
