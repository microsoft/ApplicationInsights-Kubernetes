# Application Insights Kubernetes Example (Worker)

## Build docker image

```shell
docker build -t workerapp .
```

* Optionally run it locally

```shell
docker run -d --name myapp workerapp
docker logs myapp
```

* Delete the container when the test is done

```shell
docker rm myapp -f
```

# Push the image when it is ready

* Tag the image properly, for example:

```shell
docker tag workerapp dockerhub_account_name/ai-k8s-worker-example:0.0.1
```

* Push the image:

```shell
docker push saars/ai-k8s-worker-example:0.0.1
```