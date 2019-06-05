# Walk-through

## Deprecated

This example is deprecated and won't be updated anymore. Please refer the [walk-through for ASP.NET Core 2.1](../BasicUsage_clr21_RBAC/Readme.md) instead.

## Description

This example walks through the necessary steps to deploy an ASP.NET Core 2.0 MVC application to Kubernetes cluster with `Application Insights for Kubernetes` on.

_Note:_ This is a simple example that does not follow all best practices, including security-related best practices. E.g. Application Insights instrumentation key is not adequately protected (it should be deployed as a secret).

* Let's start by creating an ASP.NET Core MVC application:

```shell
dotnet new mvc
```

* Add the NuGet Packages:

```shell
dotnet add package Microsoft.ApplicationInsights.AspNetCore
dotnet add package Microsoft.ApplicationInsights.Kubernetes
```

* Edit the project file, add the following property:

```xml
<PublishWithAspNetCoreTargetManifest>false</PublishWithAspNetCoreTargetManifest>
```

Reference [ASP.NET Core implicit store](https://docs.microsoft.com/en-us/dotnet/core/deploying/runtime-store#aspnet-core-implicit-store) for more details with regarding this property.

* Enable Application Insights for Kubernetes in Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddApplicationInsightsTelemetry();
    services.AddApplicationInsightsKubernetesEnricher();
    ...
}
```

* Optionally, update the base images:

```shell
docker pull microsoft/aspnetcore-build:2.0
docker pull microsoft/aspnetcore:2.0
```

* Add [Dockerfile](Dockerfile) to the project folder. Build the docker container (dockeraccount/aik8sbasic, for example) using [Dockerfile](Dockerfile) and upload it to an image registry.

```shell
docker build . -t dockeraccount/aik8sbasic:latest
docker push dockeraccount/aik8sbasic:latest
```

* Create the Kubernetes spec for the deployment and the service. Referencing [k8s.yaml](k8s.yaml). Please update the variable of `APPINSIGHTS_INSTRUMENTATIONKEY` to your own application insights instrumentation key.

Deploy it:

```shell
kubectl create -f k8s.yaml
```
