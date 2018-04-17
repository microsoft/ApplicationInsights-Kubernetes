# Walkthrough
This example walks through the necessary steps to create an ASP.NET Core 2.0 MVC application to Kubernets cluster with `Application Insights for Kubernetes` on.

_Note: This is an example, not a best practice. You will need to consider more around the security aspects like how to protect the application insights instrumentation key and so on._

* Let's start by creating an ASP.NET Core MVC applicaiton:
```
dotnet new mvc
```
* Add the NuGet Packages:
```
dotnet add package Microsoft.ApplicationInsights.AspNetCore
dotnet add package Microsoft.ApplicationInsights.Kubernetes --version 1.0.0-beta6
```

* Edit the project file, add the following property:
```
<PublishWithAspNetCoreTargetManifest>false</PublishWithAspNetCoreTargetManifest>
```
Reference [ASP.NET Core implicit store](https://docs.microsoft.com/en-us/dotnet/core/deploying/runtime-store#aspnet-core-implicit-store) for more details with regarding this property.

* Enable Application Insights in Program.cs by calling UseApplicaitonInsights() on WebHostBuilder:
```
public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseApplicationInsights()
        .UseStartup<Startup>()
        .Build();
```
* Enable Application Insights for Kubernetes in Startup.cs:
```
public void ConfigureServices(IServiceCollection services)
{
    services.EnableKubernetes();
    services.AddMvc();
}
```
* Optionally, update the base images:
```
docker pull microsoft/aspnetcore-build:2.0
docker pull microsoft/aspnetcore:2.0
```
* Build the docker container (saars/aik8sbasic, for example) using [Dockerfile](Dockerfile) and upload it to an image registry.
```
docker build . -t saars/aik8sbasic:latest
docker push saars/aik8sbasic:latest
```
*  Create the Kubernetes spec for the deployment and the service. Referencing [k8s.yaml](k8s.yaml). Please update the variable of `APPINSIGHTS_INSTRUMENTATIONKEY` to your own application insights instrumentation key.
Deploy it:
```
kubectl create -f k8s.yaml
```
