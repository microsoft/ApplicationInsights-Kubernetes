# Application Insights for Kubernetes in Windows Container

## Introduction

Starting from 1.0.0-beta7, Application Insights for Kubernetes will support running inside the Windows Containers managed by Kubernetes. This example is intended to show you how to wire everything up. You are welcome to try this scenario. But please expect a slow response for any issue reported.

_Note: This is a simple example that does not follow all best practices, including security-related best practices. E.g. Application Insights instrumentation key is not adequately protected (it should be deployed as a secret)._

## Build the application

* Let's start by creating an ASP.NET Core MVC application:

  ```shell
  dotnet new mvc
  ```

* Add the NuGet Packages:

  ```shell
  dotnet add package Microsoft.ApplicationInsights.AspNetCore
  dotnet add package Microsoft.ApplicationInsights.Kubernetes
  ```

* If you are on .NET Core 2.0, like it is in this example, edit the [project file](AspNetCoreNano.csproj), add the following property:

  ```xml
  <PublishWithAspNetCoreTargetManifest>false</PublishWithAspNetCoreTargetManifest>
  ```

  * Reference [ASP.NET Core implicit store](https://docs.microsoft.com/en-us/dotnet/core/deploying/runtime-store#aspnet-core-implicit-store) for more details with regarding this property.
  * You don't need it for `.NET Core 2.1`+ or `.NET 6`+. Refer to [here](https://github.com/dotnet/aspnetcore/issues/3310#issuecomment-404986286) for details.


* Enable Application Insights in [Program.cs](Program.cs) by calling UseApplicationInsights() on WebHostBuilder:

  ```csharp
  public static IWebHost BuildWebHost(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
          .UseApplicationInsights() // Enable Application Insights
          .UseStartup<Startup>()
          .Build();
  ```

* Add iKey to [appsettings.json](./appsettings.json).

  ```json
    "ApplicationInsights": {
      "InstrumentationKey": "your app insights instrumentation key"
    }
  ```

* Enable Application Insights for Kubernetes in [Startup.cs](Startup.cs):

  ```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddApplicationInsightsKubernetesEnricher();
      services.AddMvc();
  }
  ```

## Build the image

* Optionally, pull the base images:

  ```shell
  docker pull microsoft/aspnetcore-build:2.0.3-nanoserver-1709
  docker pull microsoft/aspnetcore:2.0.3-nanoserver-1709
  ```

You will need to pick the base image to match your version of Windows running on your Kubernetes.

* Build the docker container (dockeraccount/aspnetcorenano, for example) using [Dockerfile](Dockerfile) and upload it to an image registry.

  ```shell
  docker build . -t dockeraccount/aspnetcorenano
  docker push dockeraccount/aspnetcorenano:latest
  ```

## Write the Kubernetes Specs

* Create the Kubernetes spec for the deployment and the service. Referencing [k8s.yaml](k8s/k8s.yaml). Be noticed, different than the Application Insights for Kubernetes running in Linux container, pod name is passed in as an environment variable at this point:

  ```yaml
  env:
    - name: APPINSIGHTS_KUBERNETES_POD_NAME
      valueFrom:
        fieldRef:
          fieldPath: metadata.name
  ```

## Setup the default Service Account for RBAC enabled cluster

* If the cluster is RBAC enabled, the service account used will need to bind to proper cluster role so that the application can fetch Kubernetes related properties. In [saRole.yaml](./k8s/saRole.yaml), a cluster role named appinsights-k8s-property-reader is created and then bind to the default service account. Permissions needed are listed in the resources property. To deploy it, update the value for the namespace and then:

  ```shell
  kubectl create -f k8s/saRole.yaml
  ```

* Please replace the `NAMESPACE` accordingly.

## Deploy it

  ```shell
  kubectl create -f k8s.yaml
  ```

## Known issues

* Multiple containers in one pod is not supported.
