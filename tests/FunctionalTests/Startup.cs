using System.Diagnostics;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalTests
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add these lines for diagnostics of AI.K8s library.
            var listener = InspectListener.Instance;
            listener.SetMinimumLevel(InspectLevel.Trace);
            Inspect.Instance.Observer.SubscribeWithAdapter(listener);

            // Enable telemetry for Application Insights.
            services.AddApplicationInsightsTelemetry();

            // Add application insights for K8s with k8s environment simulators.
            services.AddApplicationInsightsKubernetesEnricher(applyOptions: null,
                kubernetesServiceCollectionBuilder: new KubernetesDebuggingServiceCollectionBuilder(),
                detectKubernetes: () => true
            );
            // Or enable application insights for K8s
            // services.AddApplicationInsightsKubernetesEnricher();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
