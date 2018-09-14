using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HostBuilderExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder().ConfigureAppConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                }).ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IHostedService, PrintHelloService>();
                }).UseConsoleLifetime().Build();

            host.Run();
        }
    }
}
