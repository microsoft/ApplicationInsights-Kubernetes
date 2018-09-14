using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationInsights.Kubernetes.HostingStartup
{
    /// <summary>
    /// Hosted Startup point for Application Insights for Kubernetes.
    /// </summary>
    public class K8sInjection : IHostingStartup
    {
        /// <summary>
        /// Configure the Application Insights for Kubernetes.
        /// </summary>
        /// <param name="builder">The web host builder.</param>
        public void Configure(IWebHostBuilder builder)
        {
            builder.UseApplicationInsights()
                .ConfigureServices(services =>
                {
                    services.EnableKubernetes();
                });
        }
    }
}
