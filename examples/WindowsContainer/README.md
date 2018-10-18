Starting from 1.0.0-beta7, Application Insights for Kubernetes will support running inside the Windows Containers managed by Kubernetes. This example is intended to show you how to wire everything up. You are welcome to try this scenario. But please expect a slow response for any issue reported.

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

* Edit the [project file](AspNetCoreNano.csproj), add the following property:
```
<PublishWithAspNetCoreTargetManifest>false</PublishWithAspNetCoreTargetManifest>
```
Reference [ASP.NET Core implicit store](https://docs.microsoft.com/en-us/dotnet/core/deploying/runtime-store#aspnet-core-implicit-store) for more details with regarding this property.

* Enable Application Insights in [Program.cs](Program.cs) by calling UseApplicaitonInsights() on WebHostBuilder:
```
public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseApplicationInsights()
        .UseStartup<Startup>()
        .Build();
```
* Add iKey to the [appsettings.json](appsettings.json).
```json
  "ApplicationInsights": {
    "InstrumentationKey": "your app insights instrumentation key"
  }
```
* Enable Application Insights for Kubernetes in [Startup.cs](Startup.cs):
```
public void ConfigureServices(IServiceCollection services)
{
    services.EnableKubernetes();
    services.AddMvc();
}
```
* Optionally, pull the base images:
```
docker pull microsoft/aspnetcore-build:2.0.3-nanoserver-1709
docker pull microsoft/aspnetcore:2.0.3-nanoserver-1709
```
You will need to pick the base image to match your version of Windows running on your Kubernetes.

* Build the docker container (dockeraccount/aspnetcorenano, for example) using [Dockerfile](Dockerfile) and upload it to an image registry.
```
docker build . -t dockeraccount/aspnetcorenano
docker push dockeraccount/aspnetcorenano:latest
```
* Create the Kubernetes spec for the deployment and the service. Referencing [k8s.yaml](k8s/k8s.yaml). Be noticed, different than the Application Insights for Kubernetes running in Linux container, pod name is passed in as an environment variable at this point:
```yaml
        env:
          - name: APPINSIGHTS_KUBERNETES_POD_NAME
            valueFrom:
              fieldRef:
                fieldPath: metadata.name
```
Deploy it:
```
kubectl create -f k8s.yaml
```

Known issues:
* Multiple containers in one pod is not supported.
