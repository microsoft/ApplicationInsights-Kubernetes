# How to deploy to K8s environment

## Build docker image

1. Set docker hub account, for example
    ```shell
    docker_account=your_docker_hub_account_name
    ```

1. Build `Release` locally

    ```shell
    dotnet publish -c Release
    ```

1. Build docker image

    ```shell
    docker build -t f5webapi .
    ```

1. Test it locally

    ```shell
    docker run -it f5webapi
    ```

1. Tag the image

    ```shell
    docker tag f5webapi $docker_account/f5webapi
    ```

1. Push the image

    ```shell
    docker push $docker_account/f5webapi
    ```

1. Deploy a namespace if it isn't there already:

    ```shell
    kubectl create -f ./k8s-namespace.yml
    ```

1. Update [K8s.yml](./k8s.yml) with the proper image name, then deploy

    ```shell
    kubectl create -f ./k8s.yml
    ```
