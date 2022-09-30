# Zero Code light up Example

This example shows how to enable application insights for Kubernetes for ASP.NET Core 6.x WebAPI without code change for the existing project.

It can be accomplished in 2 steps:

* Add the proper NuGet Package
* Add Environment Variables

The example below shows the details.

## Reference hosting startup package in Dockerfile

Add this line in the [Dockerfile](./dockerfile):

```dockerfile
...
# Adding a reference to hosting startup package. Replace --prerelease with the target version of the package you want to use.
RUN dotnet add package Microsoft.ApplicationInsights.Kubernetes.HostingStartup --prerelease

# Light up Application Insights for Kubernetes using hosting startup.
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.ApplicationInsights.Kubernetes.HostingStartup
...
```

Notes: for the environment variable of `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`, you could alternatively add it in Kubernetes yaml file as well. For example, in [k8s.yaml](./k8s.yaml).

Here's the the full [Dockerfile](./dockerfile) example.

*Note: To make your build context as small as possible add a [.dockerignore](./.dockerignore) file to your project folder.*

## Build and run the Docker image

1. Open a command prompt and navigate to your project folder.

1. Use the following commands to build and run your Docker image:

    ```shell
    docker build -t ai-k8s-app .
    docker container rm test-ai-k8s-app -f # Making sure any existing container with the same name is deleted
    docker run -d -p 8080:80 --name test-ai-k8s-app ai-k8s-app
    docker logs test-ai-k8s-app
    ```

1. Expect the error

    If you follow the steps above correctly, you shall see the following error in the beginning of the log:

    ```shell
    [Error] [2022-06-10T21:23:10.0412107Z] Application is not running inside a Kubernetes cluster.
    ```

    Congratulations, your image is ready to run inside the Kubernetes! The error, as a matter of fact, is a good sign that Application Insights for Kubernetes is injected and it is trying to get the Kubernetes related data. It failed only because it is running outside of the Kubernetes.

1. Delete the running container after the verification is done:

    ```shell
    docker container rm test-ai-k8s-app -f
    ```

## Upload the image to container registry

Once verified, the image is ready to be uploaded. Here we use docker hub for example, it works with any Docker image registry:

```shell
docker tag ai-k8s-app:latest dockeraccount/ai-k8s-app:latest
docker push dockeraccount/ai-k8s-app:latest
```

**Note:** Change the tag properly. For more details, please reference the docker document: [Repositories](https://docs.docker.com/docker-hub/repos/).

## Deploy the application in Kubernetes

Now that the image is in the container registry, it is time to deploy it to Kubernetes.

* You could use default namespace, but it is recommended to put the test application in a dedicated namespace, for example 'ai-k8s-demo'. To deploy a namespace, use content in [k8s-namespace.yaml](../k8s-namespace.yaml):

    ```
    kubectl create -f k8s-namespace.yaml
    ```

* Create a secret for application insights connection string

    Create & deploy a secret using the following yaml.

    ```yaml
    apiVersion: v1
    kind: Secret
    metadata:
      name: app-env-secret
      namespace: ai-k8s-demo # Update to your namespace
    type: Opaque
    data:
      appinsights-connection-string: eW91IGhhdmUgdG8gZG8gdGhpcywgZG9uJ3QgeW91Pw==   #Base64 encoded connection string.
    ```
    _Tips: Connection string can be fetched from the Azure Portal. See [connection strings](https://docs.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string?tabs=net) for more details._

    _Tips: if you are going to create a file in your repository, `secrets.yaml` for example, adding the path to `.gitignore` to avoid checking it in by accident._

    For example, you may deploy it like this:

    ```shell
    kubectl create -f .\secrets.yaml
    ```

    Refer to [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/) for more info.

* Setup proper role binding for RBAC enabled clusters

    If you have RBAC enabled for your cluster, permissions need to be granted to the service account to access the cluster info for telemetries. Refer to [Configure RBAC permissions](../../docs/configure-rbac-permissions.md) for details.

    For example, deploy a role assignment in namespace of `ai-k8s-demo` by calling:

    ```shell
    kubectl create -f ..\..\docs\sa-role.yaml
    ```

    The output looks like this:

    > clusterrole.rbac.authorization.k8s.io/appinsights-k8s-property-reader created  
    clusterrolebinding.rbac.authorization.k8s.io/appinsights-k8s-property-reader-binding created

* Deploy the application

    Create a Kubernetes deployment file. Reference [k8s.yaml](k8s.yaml) as an example, pay attention to **namespace**, **image** and **environment variables**, making sure they are properly setup.

    Then run the following command to deploy the app:

    ```shell
    kubectl create -f k8s.yaml
    ```

    To learn more about Kubernetes deployment, please refer to [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/).

* Port forward for testing

    1. Get Pod name:
        
        ```shell
        kubectl get pod --namespace ai-k8s-demo
        ```
        Note down the pod name.

    2. Use port forward to hit your application. For example, listens locally on 8080, forward it to 80 in the pod:

        ```shell
        kubectl port-forward pod/<podName> 8080:80 --namespace ai-k8s-demo
        ```
    
        You will see output:

        > Forwarding from 127.0.0.1:8080 -> 80  
        Forwarding from [::1]:8080 -> 80  
        Handling connection for 8080

    3. Hit the endpoint to generate some traffic. For example:

        ```
        curl http://localhost:8080/weatherforecast
        ```
        
        After a while (around 2 minutes), you shall see Application Insights data coming with Kubernetes properties on it.


        ![Application Insights Events with Kubernetes Properties](./.media/AI_K8s_Properties.png)

## Summary

In this example, we modified the Dockerfile a bit to add the required NuGet packages into the project, then we used the environment variables to enable the feature.

We also touched the surface of using secrets to protect sensitive configurations like connection string of the application insights resource.
