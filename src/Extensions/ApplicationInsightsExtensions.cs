namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Netcore.Kubernetes;

    /// <summary>
    /// Extnesion method to inject Kubernetes Telemtry Initializer.
    /// </summary>
    public static class ApplicationInsightsExtensions
    {
        public static IServiceCollection EnableK8s(this IServiceCollection services, TimeSpan? timeout = null)
        {
            // 2 minutes maximum to spin up the container.
            timeout = timeout ?? TimeSpan.FromMinutes(2);
            K8sEnvironment k8sEnv = K8sEnvironment.CreateAsync(timeout.Value).ConfigureAwait(false).GetAwaiter().GetResult();
            // Wait until the initialization is done.
            k8sEnv.InitializationWaiter.WaitOne(TimeSpan.FromMinutes(1));

            // Inject the telemetry initializer.
            services.AddSingleton(k8sEnv);
            services.AddSingleton<ITelemetryInitializer, KubernetesTelemetryInitializer>();
            return services;
        }
    }
}
