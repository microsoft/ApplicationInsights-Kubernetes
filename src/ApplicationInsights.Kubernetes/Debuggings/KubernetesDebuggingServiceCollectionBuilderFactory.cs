using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Factory of Application Inisghts for Kubernetes debugging service collection builder.
    /// </summary>
    public sealed class KubernetesDebuggingServiceCollectionBuilderFactory
    {
        private KubernetesDebuggingServiceCollectionBuilderFactory() { }

        /// <summary>
        /// Singleton intance of the KubernetesDebuggingServiceCollectionBuilderFactory.
        /// </summary>
        public static KubernetesDebuggingServiceCollectionBuilderFactory Instance { get; } = new KubernetesDebuggingServiceCollectionBuilderFactory();

        /// <summary>
        /// Create a debugging instance of the service collection builder for Application Insights for Kubernetes.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <returns>Returns a debugging instace of the service collectino builder for Application Insights for Kubernetes.</returns>
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