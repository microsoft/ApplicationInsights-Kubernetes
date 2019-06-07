using System.Diagnostics;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace F5WebApi
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
            // To use this project for F5 debugging, follow these steps:
            
            // Output the diagnostic source logs to the console.
            var observer = new ApplicationInsightsKubernetesDiagnosticObserver(DiagnosticLogLevel.Trace);
            ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);

            // Step 1. Set iKey in the parameter below.
            services.AddApplicationInsightsTelemetry("your-instrumentation-key");

            // Step 2. Call proper overloads of AddApplicationInsightsKubernetesEnricher.
            services.AddApplicationInsightsKubernetesEnricher(applyOptions: null,
                kubernetesServiceCollectionBuilder: KubernetesDebuggingServiceCollectionBuilderFactory.Instance.Create(),
                detectKubernetes: () => true);
            // Step 3. Set a break point and press F5.
            // ~

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
