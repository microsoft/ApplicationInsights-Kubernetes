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

            // uService_prototype
            TelemetryConfiguration aiConfig = new TelemetryConfiguration("8e9838a3-ad63-4d30-96f7-2f0a505bc0f6", app.ApplicationServices.GetService<ITelemetryChannel>());
            aiConfig.EnableKubernetes();
            TelemetryClient client = new TelemetryClient(aiConfig);
            // Invoking the constructor for the TelemetryInitializer
            client.TrackEvent("Hello");
            // saarsfun01
            TelemetryConfiguration aiConfig2 = new TelemetryConfiguration("5789ad10-8b39-4f8a-88dc-632d1342d5e0", app.ApplicationServices.GetService<ITelemetryChannel>());
            aiConfig2.EnableKubernetes();
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
