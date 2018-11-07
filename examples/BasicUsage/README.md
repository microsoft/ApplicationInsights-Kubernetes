# Walkthrough
This example walks through the necessary steps to deploy an ASP.NET Core 2.0 MVC application to Kubernets cluster with `Application Insights for Kubernetes` on.

_Note: This is a simple example that does not follow all best practices, including security-related best practices. E.g. Application Insights instrumentation key is not adequately protected (it should be deployed as a secret)_

* Let's start by creating an ASP.NET Core MVC applicaiton:
```
dotnet new mvc
```
* Add the NuGet Packages:
```
dotnet add package Microsoft.ApplicationInsights.AspNetCore
dotnet add package Microsoft.ApplicationInsights.Kubernetes
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
    services.AddApplicationInsightsKubernetesEnricher();
    services.AddMvc();
}
```
* Optionally, update the base images:
```
docker pull microsoft/aspnetcore-build:2.0
docker pull microsoft/aspnetcore:2.0
```
* Add [Dockerfile](Dockerfile) to the project folder. Build the docker container (dockeraccount/aik8sbasic, for example) using [Dockerfile](Dockerfile) and upload it to an image registry.
```
docker build . -t dockeraccount/aik8sbasic:latest
docker push dockeraccount/aik8sbasic:latest
```
*  Create the Kubernetes spec for the deployment and the service. Referencing [k8s.yaml](k8s.yaml). Please update the variable of `APPINSIGHTS_INSTRUMENTATIONKEY` to your own application insights instrumentation key.
Deploy it:
```
kubectl create -f k8s.yaml
```
