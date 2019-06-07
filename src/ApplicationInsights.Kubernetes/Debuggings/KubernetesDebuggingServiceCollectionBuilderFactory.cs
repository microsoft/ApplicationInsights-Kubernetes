using System;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Factory of Application Insights for Kubernetes debugging service collection builder.
    /// </summary>
    public sealed class KubernetesDebuggingServiceCollectionBuilderFactory
    {
        private KubernetesDebuggingServiceCollectionBuilderFactory() { }
        static KubernetesDebuggingServiceCollectionBuilderFactory() { }

        /// <summary>
        /// Singleton instance of the KubernetesDebuggingServiceCollectionBuilderFactory.
        /// </summary>
        public static KubernetesDebuggingServiceCollectionBuilderFactory Instance { get; } = new KubernetesDebuggingServiceCollectionBuilderFactory();

        /// <summary>
        /// Creates a debugging instance of the service collection builder for Application Insights for Kubernetes.
        /// </summary>
        /// <returns>Returns a debugging instace of the service collection builder for Application Insights for Kubernetes.</returns>
        [Obsolete("This instance is used only for debugging. Never use this in production!", false)]
#pragma warning disable CA1822 // Mark members as static
        public KubernetesDebuggingServiceCollectionBuilder Create()
#pragma warning restore CA1822 // Mark members as static
        {
            return new KubernetesDebuggingServiceCollectionBuilder();
        }
    }
}