﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApplicationInsights.Kubernetes.HostingStartup
{
    /// <summary>
    /// Hosting Startup point for Application Insights for Kubernetes.
    /// </summary>
    public class ApplicationInsightsForK8sHostingStartup : IHostingStartup
    {
        /// <summary>
        /// Configures the Application Insights for Kubernetes.
        /// </summary>
        /// <param name="builder">The web host builder.</param>
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((cxt, services) =>
            {
                services.AddApplicationInsightsTelemetry();
                services.AddApplicationInsightsKubernetesEnricher(diagnosticLogLevel: LogLevel.Warning);
            });
        }
    }
}
