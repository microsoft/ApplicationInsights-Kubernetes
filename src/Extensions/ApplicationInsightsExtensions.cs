namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Kubernetes;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extnesion method to inject Kubernetes Telemtry Initializer.
    /// </summary>
    public static class ApplicationInsightsExtensions
    {
        public static IServiceCollection EnableK8s(this IServiceCollection services, TimeSpan? timeout = null)
        {
            ILoggerFactory loggerFactory = (ILoggerFactory)services.FirstOrDefault(s => s.ServiceType == typeof(ILoggerFactory))?.ImplementationInstance;
            KubernetesModule.EnableK8s(TelemetryConfiguration.Active, loggerFactory, timeout);
            return services;
        }
    }
}
