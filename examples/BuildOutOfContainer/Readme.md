# Example of building the application out of container

This is an example when building the application out of the container.

## Prepare this example to try it locally

There are some placeholders to update if you want to run this by yourself:

* Edit [appsettings.json](./appsettings.json), update the `ConnectionString`. You can get a connection string from Azure Portal UI on your application insights resource. Refer to [Connection strings](https://docs.microsoft.com/en-us/azure/azure-monitor/app/sdk-connection-string) for more details.

* In the following steps, when build / push the images, replace `dockerhubaccount` with your own account name. Also, update it in [K8s.yaml](./K8s.yaml) so that the image pull will success.

## Build the container

* Build/Publish the binaries locally

    ```shell
    dotnet publish -c Release
    ```

* If it is the first time running, pull the base image:

    ```shell
    docker pull mcr.microsoft.com/dotnet/aspnet:5.0
    ```

* Build the container:

    ```shell
    docker build -t dockerhubaccount/ai-k8s-oc .
    ```

## Push the built container to docker

```shell
docker push dockerhubaccount/ai-k8s-oc
```

## Create a deployment and a service

Refer to [K8s.yaml](./K8s.yaml) for an example.

* Create the deployment and the service:

    ```shell
    kubectl create -f K8s.yaml
    ```
_Notice a service is exposed on port 30007._

* Invoke the endpoints:

    ```shell
    curl http://localhost:30007/weatherforecast
    ```

* Inspect the logs of the pod

    ```shell
    kubectl get pod
    kubectl logs <podName>
    ```

## Remove the deployment afterwards for cleanup

```shell
kubectl delete -f .\K8s.yaml
```

Got:
> deployment.apps "ai-k8s-webapi" deleted  
service "ai-k8s-oc-webapi-svc" deleted  



