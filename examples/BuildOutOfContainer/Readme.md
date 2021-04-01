# Example of building the application out of container

This is an example when building the application out of the container.

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

## Create a deployment

Refer to [K8s.yaml](./K8s.yaml) for an example.

* Create the deployment: 

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



