# Application Insights for Kubernetes on WebAPI

This page is a walk-through to enable `Application Insights for Kubernetes` in WebAPI projects, including traditional WebAPI (controller-based) and minimal WebAPIs. The example code is separated in [WebAPI](./WebAPI/) and [MinimalAPI](./MinimalAPI/) respectively.

_To learn more about minimal API, please refer to the [Minimal APIs overview](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0)._

## Prerequisite

* .NET 6 SDK - <https://dot.net>
    * Verify dotnet version. For example:
        ```shell
        dotnet --version
        6.0.302
        ```
    _You might have a slightly different patch version and that should be fine._

* A Kubernetes Cluster that you can manage with `kubectl`.
    * If you don't have any, there are several options:
        * [Azure AKS](https://docs.microsoft.com/en-us/azure/aks/)
        * [Docker Desktop](https://www.docker.com/products/docker-desktop/)
        * [MiniKube](https://minikube.sigs.k8s.io/docs/start/)
        * ...
    * Verify that the credential is properly set up for `kubectl`, for example:

        ```shell
        kubectl get nodes
        NAME                        STATUS   ROLES           AGE    VERSION
        aks-nodepool1-10984277-0    Ready    agent           5d8h   v1.24.1
        ```

* A container image registry
  * The image built will be pushed into an image registry. Dockerhub is used in this example. It should work with any image registry.

## Prepare the project

1. Create the project from templates

    * For Control-Based WebAPI:

        ```shell
        dotnet new webapi           # Create a control-based webapi from the template
        ```

    * For Minimal WebAPI

        ```shell
        dotnet new web             # Create a minimal webapi from the template
        ```

2. Add NuGet packages

    ```shell
    dotnet add package Microsoft.ApplicationInsights.AspNetCore
    dotnet add package Microsoft.ApplicationInsights.Kubernetes --prerelease
    ```

## Enable Application Insights for Kubernetes

To enable Application Insights for Kubernetes, the services need to be registered:

```csharp
...
// Enable application insights
builder.Services.AddApplicationInsightsTelemetry();

// Enable application insights for Kubernetes
builder.Services.AddApplicationInsightsKubernetesEnricher(diagnosticLogLevel: LogLevel.Information);
...
```

See a full example for [WebAPI](./WebAPI/Program.cs) or [Minimal WebAPI](./MinimalAPI/Program.cs).

## Prepare the image

1. Get the **dockerfile** ready. For example, refer to [this](./WebAPI/dockerfile) for WebAPI or [this](./MinimalAPI/dockerfile) for Minimal Web API, remember to update the `ENTRYPOINT` to align with your assembly name. For more about containerizing an ASP.NET application, refer to [Dockerize an ASP.NET Core application](https://docs.docker.com/samples/dotnetcore/).

1. Setup some variables for the convenience

    ```shell
    $imageName='ai-k8s-web-api'                     # Choose your own tag name
    $imageVersion='1.0.0'                           # Update the version of the container accordingly
    $testAppName='test-ai-k8s-web-api'              # Used as the container instance name for local testing
    $imageRegistryAccount='your-registry-account'   # Used to push the image
    ```
    _Note: In this example, we use PowerShell. Tweak it accordingly in other shells. Also, you don't have to have those variables if you prefer to use the value directly below._

1. Build the image

    ```shell
    docker build -t "$imageName`:$imageVersion" .   # For PowerShell, you need to escape the colon(:) with (`).
    docker container rm $testAppName -f             # Making sure any existing container with the same name is deleted
    docker run -d -p 8080:80 --name $testAppName "$imageName`:$imageVersion"
    ```

1. Check the logs

    ```shell
    docker logs $testAppName
    ```

    When you see this error at the beginning of the logs, it means the container is ready to be put into a Kubernetes cluster:

    ```log
    [Error] [2022-08-12T23:56:08.9973664Z] Application is not running inside a Kubernetes cluster.
    ```

1. Remove the running container

    ```shell
    docker container rm $testAppName -f
    ```

1. Tag and push the image

    ```shell
    docker tag "$imageName`:$imageVersion" "$imageRegistryAccount/$imageName`:$imageVersion"
    docker push "$imageRegistryAccount/$imageName`:$imageVersion"
    ```

## Deploy to Kubernetes Cluster

Now that the image is in the container registry, it is time to deploy it to Kubernetes.

1. Deploy to a dedicated namespace

    You could use the default namespace, but it is recommended to put the test application in a dedicated one, for example, 'ai-k8s-demo'. To deploy a namespace, use content in [k8s-namespace.yaml](../k8s-namespace.yaml):

    ```shell
    kubectl create -f ..\k8s-namespace.yaml # tweak the path accordingly
    ```

1. Setup proper role binding for RBAC-enabled clusters

    If you have RBAC enabled for your cluster, permissions need to be granted to the service account to access the cluster info for telemetries. Refer to [Configure RBAC permissions](../../docs/configure-rbac-permissions.md) for details.

    For example, deploy a role assignment in the namespace of `ai-k8s-demo` by using [sa-role.yaml](../../docs/sa-role.yaml):

    ```shell
    kubectl create -f ..\..\docs\sa-role.yaml # tweak the path accordingly
    ```

    _Notes: Check the namespace for the service account in the yaml file._

1. Deploy the application

    Create a Kubernetes deployment file. Reference [k8s.yaml](./k8s.yaml) as an example, pay attention to **namespace**, **image**, and **environment variables**, making sure they are properly set up.

    Then run the following command to deploy the app:

    ```shell
    kubectl create -f k8s.yaml
    ```

    To learn more about Kubernetes deployment, please refer to [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/).

1. Check out the pod deployed:

    ```shell
    kubectl get pods --namespace ai-k8s-demo    # Or whatever your namespace of choice.
    ```

    And you will see something like this:

    ```
    PS > kubectl get pods --namespace ai-k8s-demo
    NAME                                             READY   STATUS    RESTARTS   AGE
    some-other-pod-that-has-run-for-10-days-pmfdc    1/1     Running   0          10d
    ai-k8s-web-api-deployment-97d688b46-7cxs5        1/1     Running   0          2m58s           <==== created ~3 minutes ago
    ```

    Check out the logs, for example:

    ```shell
    PS > kubectl logs ai-k8s-web-api-deployment-97d688b46-7cxs5 --namespace ai-k8s-demo
    [Information] [2022-08-23T21:32:01.0135029Z] [CGroupContainerIdProvider] Got container id: 41a149977b4ea424947d54c59e7f63d32664e19503adda762004250f72db7687
    [Information] [2022-08-23T21:32:01.2737318Z] Found pod by name providers: ai-k8s-web-api-deployment-97d688b46-7cxs5
    info: Microsoft.Hosting.Lifetime[14]
        Now listening on: http://[::]:80
    info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
    info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Production
    info: Microsoft.Hosting.Lifetime[0]
        Content root path: /app/
    ```

1. Test the endpoint

    One way to hit the endpoint is by port forwarding. Check out the example in [Deploy the application in Kubernetes](https://github.com/microsoft/ApplicationInsights-Kubernetes/blob/develop/examples/ZeroUserCodeLightup.Net6/README.md#deploy-the-application-in-kubernetes), looking for "Port forward for testing" section specifically.

1. Delete the cluster after the test

    ```
    PS > kubectl delete -f k8s.yaml
    deployment.apps "ai-k8s-web-api-deployment" deleted
    ```
