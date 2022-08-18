# Application Insights for Kubernetes on WebAPI

This page is a walk-though to enable `Application Insights for Kubernetes` in WebAPI project, including traditional WebAPI (controller-based) and minimal WebAPIs. The example code is separated in [WebAPI](./WebAPI/) and [MinimalAPI](./MinimalAPI/) respectively.

_To learn more about minimal API, please refer to [Minimal APIs overview](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0)._

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
    * Verify that the credential is properly setup for `kubectl`, for example:

        ```shell
        kubectl get nodes
        NAME                        STATUS   ROLES           AGE    VERSION
        aks-nodepool1-10984277-0    Ready    agent           5d8h   v1.24.1
        ```

* A container image registry
  * The image built will be pushed into an image registry. Dockerhub is used in this example. It should work with any image registry.

## Prepare the project

1. Create the project from templates

    <details>
        <summary>Expand for Control-Based WebAPI</summary>

    ```shell
    dotnet new webapi           # Create a control-based webapi from the template
    ```
    </details>

    <details>
        <summary>Or expand me for Minimal WebAPI</summary>

    ```shell
    dotnet new web             # Create a minimal webapi from the template
    ```
    </details>

1. Add NuGet packages

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
builder.Services.AddApplicationInsightsKubernetesEnricher(LogLevel.Information);
...
```

See a full example for [WebAPI](./WebAPI/Program.cs) or [Minimal WebAPI](./MinimalAPI/Program.cs).

## Prepare the image

1. Setup some variables for the convenience

    ```shell
    $imageName='ai-k8s-web-api'                     # Choose your tag name
    $imageVersion='1.0.0'                           # Update the version of the container accordingly
    $testAppName='test-ai-k8s-web-api'              # Used as the container instance name for local testing
    $imageRegistryAccount='your-registry-account'   # Used to push the image
    ```
    _Note: That's an example in PowerShell. Tweak it accordingly in other shell like bash. You don't have to have those variables either._

1. Build the image

    This is a shorthands to build docker image for this example:

    ```shell
    docker build -t "$imageName`:$imageVersion" .   # For powershell, you need to escape the colon(:).
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
    docker container rm test-ai-k8s-minimal-api -f
    ```

1. Tag and push the image

    ```shell
    docker tag ai-k8s-minimal-api:latest registry_account/ai-k8s-minimal-api:latest
    docker push registry_account/ai-k8s-minimal-api:latest
    ```


## Deploy to Kubernetes Cluster

Now that the image is in the container registry, it is time to deploy it to Kubernetes.

1. Deploy to dedicated namespace

    You could use default namespace, but it is recommended to put the test application in a dedicated namespace, for example 'ai-k8s-demo'. To deploy a namespace, use content in [k8s-namespace.yaml](../k8s-namespace.yaml):

    ```shell
    kubectl create -f ..\k8s-namespace.yaml
    ```

1. Setup proper role binding for RBAC enabled clusters

    If you have RBAC enabled for your cluster, permissions need to be granted to the service account to access the cluster info for telemetries. Refer to [Configure RBAC permissions](../../docs/configure-rbac-permissions.md) for details.

    For example, deploy a role assignment in namespace of `ai-k8s-demo` by calling:

    ```shell
    kubectl create -f ..\..\docs\sa-role.yaml
    ```

1. Deploy the application

    Create a Kubernetes deployment file. Reference [k8s.yaml](./k8s.yaml) as an example, pay attention to **namespace**, **image** and **environment variables**, making sure they are properly setup.

    Then run the following command to deploy the app:

    ```shell
    kubectl create -f k8s.yaml
    ```

    To learn more about Kubernetes deployment, please refer to [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/).
