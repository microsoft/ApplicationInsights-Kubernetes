# Application Insights for Kubernetes on Minimal WebAPI

There is nothing special to use Minimal WebAPI with Application Insights for Kubernetes are the same.

More about minimal API, refer to [Minimal APIs overview](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0);

## Prerequisite

* .NET 6 SDK - <https://dot.net>
    * Verify dotnet version by:
        ```shell
        dotnet --version
        6.0.302
        ```
* A Kubernetes Cluster that you can manage with `kubectl`.
    * If you don't have any, there are several options:
        * [Azure AKS](https://docs.microsoft.com/en-us/azure/aks/)
        * [Docker Desktop](https://www.docker.com/products/docker-desktop/)
        * [MiniKube](https://minikube.sigs.k8s.io/docs/start/)
        * ...
    * Verify that the credential is properly set for `kubectl`, for example:

        ```shell
        kubectl get nodes
        NAME                        STATUS   ROLES           AGE    VERSION
        aks-nodepool1-10984277-0    Ready    agent           5d8h   v1.24.1
        ```

* A container image repository
  * The image built will be pushed into an image repository. Dockerhub is used in this example.

## Prepare the project

1. Create the project by running:

    ```shell
    dotnet new web
    ```

1. Add NuGet packages

    ```shell
    dotnet add package Microsoft.ApplicationInsights.AspNetCore
    dotnet add package Microsoft.ApplicationInsights.Kubernetes --prerelease
    ```

## Enable Application Insights for Kubernetes

To enable Application Insights for Kubernetes, the services need to be registered in [Program.cs](./Program.cs):

```csharp
...
// Enable application insights
builder.Services.AddApplicationInsightsTelemetry();

// Enable application insights for Kubernetes
builder.Services.AddApplicationInsightsKubernetesEnricher(LogLevel.Information);
...
```

## Prepare the image

1. Build the image

    This is a shorthands for you to build docker image for this example:

    ```shell
    docker build -t ai-k8s-minimal-api .
    docker container rm test-ai-k8s-minimal-api -f # Making sure any existing container with the same name is deleted
    docker run -d -p 8080:80 --name test-ai-k8s-minimal-api ai-k8s-minimal-api
    docker logs test-ai-k8s-minimal-api
    ```

    When you see this error at the beginning of the logs, it means the container is ready to be put into a Kubernetes cluster:

    ```log
    [Error] [2022-08-12T23:56:08.9973664Z] Application is not running inside a Kubernetes cluster.
    ```

1. Remove the running container

    ```shell
    docker container rm test-ai-k8s-minimal-api -f
    ```

2. Tag and push the image

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
