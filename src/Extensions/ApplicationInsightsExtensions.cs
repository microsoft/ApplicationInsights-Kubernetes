namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Kubernetes;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extnesion method to inject Kubernetes Telemtry Initializer.
    /// </summary>
    public static class ApplicationInsightsExtensions
    {
        public static IServiceCollection EnableKubernetes(this IServiceCollection services, TimeSpan? timeout = null)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            KubernetesModule.EnableKubernetes(TelemetryConfiguration.Active, loggerFactory, timeout);
            return services;
        }
    }
}
