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
            // 2 minutes maximum to spin up the container.
            timeout = timeout ?? TimeSpan.FromMinutes(2);
            ILoggerFactory loggerFactory = (ILoggerFactory)services.FirstOrDefault(s => s.ServiceType == typeof(ILoggerFactory))?.ImplementationInstance;

            K8sEnvironment k8sEnv = K8sEnvironment.CreateAsync(timeout.Value, loggerFactory).ConfigureAwait(false).GetAwaiter().GetResult();
            // Wait until the initialization is done.
            k8sEnv.InitializationWaiter.WaitOne(TimeSpan.FromMinutes(1));

            // Inject the telemetry initializer.
            services.AddSingleton<IK8sEnvironment>(k8sEnv);
            services.AddSingleton<ITelemetryInitializer, KubernetesTelemetryInitializer>();
            return services;
        }
    }
}
