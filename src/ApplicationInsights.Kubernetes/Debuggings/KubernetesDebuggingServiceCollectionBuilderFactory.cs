using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    public sealed class KubernetesDebuggingServiceCollectionBuilderFactory
    {
        private KubernetesDebuggingServiceCollectionBuilderFactory() { }
        public static KubernetesDebuggingServiceCollectionBuilderFactory Instance { get; } = new KubernetesDebuggingServiceCollectionBuilderFactory();

        [Obsolete("This instance is used only for debugging. Never use this in production!", false)]
#pragma warning disable CA1822 // Mark members as static
        public KubernetesDebuggingServiceCollectionBuilder Create(ILogger<KubernetesDebuggingServiceCollectionBuilder> logger)
#pragma warning restore CA1822 // Mark members as static
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            return new KubernetesDebuggingServiceCollectionBuilder(logger);
        }
    }
}