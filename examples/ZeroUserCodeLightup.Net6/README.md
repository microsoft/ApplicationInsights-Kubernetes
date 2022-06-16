# Zero Code light up Example

This example shows how to enable application insights for Kubernetes for ASP.NET Core 6.x WebAPI without code change for the existing project.

## Add Dockerfile

All you need to do is to add a few lines in the [Dockerfile](./dockerfile), and they are:

```dockerfile
...
# Adding a reference to hosting startup package
RUN dotnet add package Microsoft.ApplicationInsights.Kubernetes.HostingStartup --prerelease

# Light up Application Insights for Kubernetes using hosting startup.
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.ApplicationInsights.Kubernetes.HostingStartup
...
```

Reference the full [Dockerfile](./dockerfile).

*Note: To make your build context as small as possible add a [.dockerignore](./.dockerignore) file to your project folder.*

## Build and run the Docker image

1. Open a command prompt and navigate to your project folder.

1. Use the following commands to build and run your Docker image:

    ```shell
    docker build -t ai-k8s-app .
    docker container rm test-ai-k8s-app -f # Making sure any existing container with the same name is deleted
    docker run -d -p 8080:80 --name test-ai-k8s-app ai-k8s-app -e APPLICATIONINSIGHTS_CONNECTION_STRING="your-connection-string"
    docker logs test-ai-k8s-app
    ```

    _Tips: Connection string can be fetched from the Azure Portal. See [connection strings](https://docs.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string?tabs=net) for more details._


1. Delete the running container after the verification is done:

    ```shell
    docker container rm test-ai-k8s-app -f
    ```

## Expect the error

If you follow the steps above, you shall see the following error in the beginning of the log:

```shell
[Error] [2022-06-10T21:23:10.0412107Z] Application is not running inside a Kubernetes cluster.
```

Congratulations, your image is ready to run inside the Kubernetes! The error, as a matter of fact, is a good sign that Application Insights for Kubernetes is injected and is trying to get the Kubernetes related data. It failed only because it is running outside of the Kubernetes.

## Upload the image to container registry

Once verified, the image is ready to be uploaded. Take docker hub for example, should work with any docker image registry:

```shell
docker tag ai-k8s-app:latest dockeraccount/ai-k8s-app:0.0.1
docker push dockeraccount/ai-k8s-app:0.0.1
```

**Note:** Change the tag properly. For more details, please reference the docker document: [Repositories](https://docs.docker.com/docker-hub/repos/).

## Deploy the application in Kubernetes

Now that the image is in the container registry, it is time to deploy it to Kubernetes.

* Create a secret for application insights connection string

    Create & deploy a secret using the following yaml.

    ```yaml
    apiVersion: v1
    kind: Secret
    metadata:
    name: app-env-secret
    type: Opaque
    data:
    appinsights-connection-string: eW91ci1pa2V5LW5vdC1taW5l   #Base64 encoded connection string.
    ```

    _Tips: if you are going to create a file in your repository, `secrets.yaml` for example, adding the path to `.gitignore` to avoid check it in by accident._

    For example, you may deploy it like this:

    ```shell
    kubectl create -f .\secrets.yaml
    ```

    Refer to [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/) for more info.

* Deploy the application

    Create a Kubernetes deployment file. Reference [k8s.yaml](k8s.yaml) as an example, pay attention to the **image** and **environment variables**, making sure they are properly setup.


    Then run the following kubectl command to deploy the app:

    ```shell
    kubectl create -f deployment.yaml
    ```

    To learn more about Kubernetes deployment, please refer to [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/).

Generate some traffic to your application. After a while (around 2 minutes), you shall see Application Insights data coming with Kubernetes properties on it.

![Application Insights Events with Kubernetes Properties](./.media/AI_K8s_Properties.png)

## Summary

In this example, we modified the Dockerfile a bit to add the required NuGet packages into the project, then we used the environment variables to enable the feature.

The environment variable could also be managed by Kubernetes in the deployment spec.

We also touched the surface of using secrets to protect sensitive configurations like connection string of the application insights resource.

