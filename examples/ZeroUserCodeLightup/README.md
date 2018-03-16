# Zero Code light up Example
This walkthrough shows how to enable application insights for Kubernetes for ASP.NET Core 2.0 Web App without code change for the existing project.

## Add Dockerfile

All you need to do is to add a few lines in the [Dockerfile](./Dockerfile), and they are:

```dockerfile
...
# Adding a reference to hosting startup package
RUN dotnet add package Microsoft.ApplicationInsights.Kubernetes.HostingStartup  -v 1.0.0-*

# Light up Application Insights for Kubernetes
ENV APPINSIGHTS_INSTRUMENTATIONKEY $APPINSIGHTS_KEY
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES Microsoft.ApplicationInsights.Kubernetes.HostingStartup
...
```
Reference the full [Dockerfile](./Dockerfile).

*Note: This applies to **1.0.6-beta1** and above.*

*Note: To make your build context as small as possible add a [.dockerignore](./.dockerignore) file to your project folder.*

## Build and run the Docker image
1. Open a command prompt and navigate to your project folder.
2. Use the following commands to build and run your Docker image:
```
$ docker build -t ai-k8s-app --build-arg APPINSIGHTS_KEY=YOUR_APPLICATION_INSIGHTS_KEY .
$ docker run -p 8080:80 --name test-ai-k8s-app ai-k8s-app
```

## Expect the error
If you follow the steps above, you shall see the following error in the begining of the log:
```
fail: Microsoft.ApplicationInsights.Kubernetes.KubernetesModule[0]
      System.IO.FileNotFoundException: File contains namespace does not exist.
      File name: '/var/run/secrets/kubernetes.io/serviceaccount/namespace'
```
Congratulations, your image is ready to run inside the Kubernetes! The failure, as a matter of fact, is a good sign that Application Insights for Kubernetes is injected and is trying to get the Kubernetes related data. It failed only because it is running outside of the Kubernetes.

## Summary 
In this example, we modified the Dockerfile a bit to add the required NuGet packages into the project, then we used the environment variables to enable the feature.
The environment variable could also be managed by Kubernetes in the deployment spec.

Please also consider to protect the application insights instrumentation key by using various meanings, for example, by using the [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/).
