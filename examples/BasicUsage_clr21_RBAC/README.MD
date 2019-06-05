# Walk-through

This example walks through the steps to deploy an ASP.NET Core 2.1 MVC application to a Kubernetes cluster with `Application Insights for Kubernetes` on. The steps also applies for ASP.NET Core 2.2 applications.

A simple cluster role sample yaml is also included to describe how to make it work in a Role-based access control(RBAC) enabled clusters.

_Tip: [Read this for more information about RBAC](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)._

_Note: This is an example that does NOT follow all best practices, including security-related best practices. E.g. Application Insights instrumentation key is not adequately protected (it should be deployed as a secret)._

## Prerequisite

* .NETCore SDK 2.1.300 or above

  * .NET Core SDK is required in this example. Go to [https://dot.net](https://dot.net) to download the latest SDK. Make sure you have `2.1.300` or `above`:

    ```shell
    dotnet --version
    2.1.301
    ```

* A Kubernetes Cluster that you can manage with kubectl.
  * If you don't have any, an easy way is to go to [Azure AKS](https://docs.microsoft.com/en-us/azure/aks/) to get a managed cluster. Verify that the credential is properly set for kubectl to work:

    ```shell
    user@user-pc:~$ kubectl get nodes
    NAME                       STATUS    ROLES     AGE       VERSION
    aks-nodepool1-10984277-0   Ready     agent     17d       v1.9.9
    aks-nodepool1-10984277-1   Ready     agent     17d       v1.9.9
    aks-nodepool1-10984277-2   Ready     agent     17d       v1.9.9
    user@user-pc:~$
    ```

* A container image repository
  * The image built will be pushed into an image repository. Dockerhub is used in this example.

## Create the project

* Let's start by creating an ASP.NET Core MVC applicaiton:

```shell
dotnet new mvc
```

* Add the NuGet Packages:

```shell
dotnet add package Microsoft.ApplicationInsights.AspNetCore
dotnet add package Microsoft.ApplicationInsights.Kubernetes
```

* Enable **Application Insights** and **Application Insights for Kubernetes** in [Startup.cs](app/Startup.cs):

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddApplicationInsightsTelemetry();             // Enables Application Insights
    services.AddApplicationInsightsKubernetesEnricher();    // Enables Application Insights for Kubernetes.
    ...
}
```

## Prepare the container

* It is optional but recommended to update the base images:

```shell
docker pull mcr.microsoft.com/dotnet/core/sdk:2.1
docker pull mcr.microsoft.com/dotnet/core/aspnet:2.1
```

* Add [Dockerfile](app/Dockerfile) to the project folder. Build the docker container (dockeraccount/aik8sbasic_rbac, for example) using [Dockerfile](app/Dockerfile) and upload it to an image registry.

```shell
docker build . -t dockeraccount/aik8sbasic_rbac:latest
docker push dockeraccount/aik8sbasic_rbac:latest
```

## Setup the default Service Account for RBAC enabled cluster

* If the cluster is RBAC enabled, the service account used will need to bind to proper cluster role so that the application can fetch Kubernetes related properties.

In [saRole.yaml](k8s/saRole.yaml), a cluster role named `appinsights-k8s-property-reader` is created and then bind to the default service account. Permissions needed are listed in the resources property. To deploy it, update the value for the `namespace` and then:

```shell
kubectl create -f k8s/saRole.yaml
```

## Deploy the application

* Create the Kubernetes spec for the deployment and the service. Referencing [k8s.yaml](k8s/k8s.yaml). Please update the variable of `APPINSIGHTS_INSTRUMENTATIONKEY` to your own application insights instrumentation key.

Deploy it:

```shell
kubectl create -f k8s/k8s.yaml
```

## Verification

Once properly set up, your telemetry data will all be decorated with Kubernetes properties on it:
![Result](media/Result.png)

## Next step

* [Troubleshoot Application Insights for Kubernetes](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/%5BAdvanced%5D-How-to-enable-self-diagnostics-for-ApplicationInsights.Kubernetes)

* [Enable Application Insights Profiler](https://github.com/Microsoft/ApplicationInsights-Profiler-AspNetCore) to optimize the performance for your application.
