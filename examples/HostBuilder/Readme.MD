# Enable Application Insights for Kubernetes in Generic HostBuilder in .NET Core Microservices
The generic `host builder` and `host` are provided in .NET Core 2.1. The host builder configures and launches a host. The host is responsible for app startup and lifetime management. The concept isn't new in ASP.NET Core world but is extended to the generic `.NET Core Applications`.

This is a great model for building a microservice that doesn't serve HTTP requests.

_Find out more about [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-2.1)._

This example is intended to demonstrate how to turn on Application Insights for Kubernetes when the application is a .NET generic hosted app.

It is assumed that you are familiar with how to do it with an [ASP.NET Core 2.1 applications](https://github.com/Microsoft/ApplicationInsights-Kubernetes/tree/develop/examples/BasicUsage_clr21_RBAC).


## Create the Service
To make use of the application host, we created a service named [PrintHellowService](./src/PrintHelloService.cs). It implements the interface of IHostedService:
```csharp
...
using Microsoft.Extensions.Hosting;

namespace HostBuilderExample
{
    public class PrintHelloService : IHostedService
    {
        TelemetryClient _client;
        Timer _asyncTimer;
        public PrintHelloService(TelemetryClient telemetryClient)
        {
            this._client = telemetryClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _asyncTimer = new Timer(state =>
            {
                string message = "Hello Hosted Service!!!";
                this._client.TrackEvent(new EventTelemetry(message));
                this._client.Flush();
            }, null, 0, 30 * 1000);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // ... Any cleanup.
            return Task.CompletedTask;
        }
    }
}
```
`StartAsync` will be invoked upon the service's started; `StopAsync` will be invoked when the service is shutdown; The constructor takes TelemetryClient object as the parameter and that will be used to send data to Application Insights.

In the example, as you can see, we send the message of `Hello Hosted Service!!!` to the Application Insights every 30 seconds.

## Inject the Service
Now that we have the service, we need to inject it into the service collection. Please refer the full code in [Program.cs](./src/Program.cs). You can find the outline below:
```csharp
...
using Microsoft.Extensions.Hosting;
using WebHosting = Microsoft.AspNetCore.Hosting;

namespace HostBuilderExample
{
    class Program
    {
        static void Main(string[] args)
        {
            IHost host = new HostBuilder().ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                configHost.AddJsonFile("hostsettings.json", optional: false);
                configHost.AddEnvironmentVariables();
            }).ConfigureServices((hostContext, services) =>
            {
                // Enable console logging for ILogger
                services.AddLogging(cfg => { cfg.AddConsole(); });
                // Microsoft.AspNetCore.Hosting.IHostingEnvironment is required by Application Insights SDK
                services.AddSingleton<WebHosting.IHostingEnvironment>(provider => hostContext.HostingEnvironment.ToAspNetCoreHostingEnvironment());
                services.AddApplicationInsightsTelemetry();
                // Enable Application Insights for Kubernetes.
                services.EnableKubernetes();

                // Inject the service.
                services.AddSingleton<IHostedService, PrintHelloService>();
            })
            .UseConsoleLifetime()
            .Build();

            host.Run();
        }
    }
}
```
There are some interesting points here:
* In the `ConfigurateHostConfiguration()` method:
  * We use reflection to get the executable folder. That is due to a discussion [here](https://github.com/dotnet/project-system/issues/589) on github.
  * We make up the configuration file named [hostsettings.json](./src/hostsettings.json) just for fun. This turns out a big fun later in these 2 senses:
    * hostsettings.json is not copied by default by the build. This causes crashes when the settings file is set to be required. We updated the project file ([HostBuilderExample.csproj](./src/HostBuilderExample.csproj)) to copy `*.json` file to the output folder.
    * The Application Insights SDK uses the hardcoded name of `appsettings.json` and `appsettings.{EnvironmentName}.json`. Putting application insights related settings in hostsettings.json will not just work. We end up having to add back an [appsettings.json](./src/appsettings.json) and put application insights settings there.
* In `ConfigureServices()` method:
  * Microsoft.AspNet.Hosting.IHostingEnvironment needs to be injected and is required by application insights while what we have, `hostContext.HostingEnvironment` is a Microsoft.Extensions.Hosting.IHostingEnvironment object. By testing, it doesn't implement both interfaces. We end up creating a wrapper extension method: [ToAspNetCoreHostingEnvironment()](./src/Extensions.cs).


## Deploy the Service
Once these hoops are jumped, everything just turns out easy. Follow the routine to create the [Dockerfile](./src/Dockerfile) to build the image and then push it:
```
docker build . -t dockerrepo/ai_k8s_selfhost:latest
docker push dockerrepo/ai_k8s_selfhost:latest
```
And create a yaml to deploy it to a K8s cluster like this one: [k8s.yaml](./k8s/k8s.yaml).

Once succeeded configured, you will see logs like this with your container:
```logs
> kubectl logs ai-k8s-selfhost-57ff68f6b8-gqdx5
?[40m?[32minfo?[39m?[22m?[49m: Microsoft.Extensions.DependencyInjection.KubernetesServiceCollectionBuilder[0]
      ApplicationInsights.Kubernetes.Version:ai-k8s:1.0.0.9
Application started. Press Ctrl+C to shut down.
Hosting environment: Development
Content root path: /app/
Hello Hosted Service!!!
Hello Hosted Service!!!
...
```

Or if you run it outside of a Kubernetes cluster, you will see a warning:
```
info: Microsoft.Extensions.DependencyInjection.KubernetesServiceCollectionBuilder[0]
      ApplicationInsights.Kubernetes.Version:ai-k8s:1.0.0.9
warn: Microsoft.Extensions.DependencyInjection.KubernetesServiceCollectionBuilder[0]
      Application is not running inside a Kubernetes cluster.
...
```

## Recap
The generic .NET host is a great model for building a microservice. In this example, we built a service, injected it into the service collection and published it, with Application Insights for Kubernetes turned on.

Without specific instruments, you will get Kubernetes related information on every telemetry item.