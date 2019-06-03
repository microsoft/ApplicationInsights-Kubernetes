using System;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Factory of Application Insights for Kubernetes debugging service collection builder.
    /// </summary>
    public sealed class KubernetesDebuggingServiceCollectionBuilderFactory
    {
        private KubernetesDebuggingServiceCollectionBuilderFactory() { }

        /// <summary>
        /// Singleton instance of the KubernetesDebuggingServiceCollectionBuilderFactory.
        /// </summary>
        public static KubernetesDebuggingServiceCollectionBuilderFactory Instance { get; } = new KubernetesDebuggingServiceCollectionBuilderFactory();
    }
}