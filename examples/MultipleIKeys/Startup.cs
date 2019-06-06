using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MultipleIKeys
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
            services.AddMvc();
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
                app.UseExceptionHandler("/Home/Error");
            }

            // Application Insights 1
            TelemetryConfiguration aiConfig = new TelemetryConfiguration("your iKey 1", app.ApplicationServices.GetService<ITelemetryChannel>());
            aiConfig.AddApplicationInsightsKubernetesEnricher(applyOptions: null);
            TelemetryClient client = new TelemetryClient(aiConfig);
            // Invoking the constructor for the TelemetryInitializer
            client.TrackEvent("Hello");
            
            // Application Insights 2
            TelemetryConfiguration aiConfig2 = new TelemetryConfiguration("your iKey 2", app.ApplicationServices.GetService<ITelemetryChannel>());
            aiConfig2.AddApplicationInsightsKubernetesEnricher(applyOptions: null);
            TelemetryClient client2 = new TelemetryClient(aiConfig2);

            var _forget = ThrowAnotherHelloAsync(client, client2);

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private async Task ThrowAnotherHelloAsync(TelemetryClient client, TelemetryClient anotherClient)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            client.TrackEvent("Hello 2");
            client.Flush();

            anotherClient.TrackEvent("Hello another");
            anotherClient.Flush();
        }
    }
}
