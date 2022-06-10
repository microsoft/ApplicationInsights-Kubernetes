**A newer version of this example is available at: [ZeroCodeLightUp.Net6](../ZeroUserCodeLightup.Net6/)**.

---

This page is not maintained anymore.

# Zero Code light up Example

This walkthrough shows how to enable application insights for Kubernetes for ASP.NET Core 2.0 Web App without code change for the existing project.

## Add Dockerfile

All you need to do is to add a few lines in the [Dockerfile](./Dockerfile), and they are:

```dockerfile
...
# Adding a reference to hosting startup package
RUN dotnet add package Microsoft.ApplicationInsights.Kubernetes.HostingStartup

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

```shell
docker build -t ai-k8s-app --build-arg APPINSIGHTS_KEY=YOUR_APPLICATION_INSIGHTS_KEY .
docker run -p 8080:80 --name test-ai-k8s-app ai-k8s-app
```

## Expect the error

If you follow the steps above, you shall see the following error in the beginning of the log:

```shell
fail: Microsoft.ApplicationInsights.Kubernetes.KubernetesModule[0]
      System.IO.FileNotFoundException: File contains namespace does not exist.
      File name: '/var/run/secrets/kubernetes.io/serviceaccount/namespace'
```

Congratulations, your image is ready to run inside the Kubernetes! The failure, as a matter of fact, is a good sign that Application Insights for Kubernetes is injected and is trying to get the Kubernetes related data. It failed only because it is running outside of the Kubernetes.

## Upload the image to the DockerHub

Once verified, the image is ready to be uploaded. For example:

```shell
docker tag ai-k8s-app:latest dockeraccount/ai-k8s-app:0.0.1
docker push dockeraccount/ai-k8s-app:0.0.1
```

**Note:** Change the tag properly. For more details, please reference the docker document: [Push images to Docker Cloud](https://docs.docker.com/docker-cloud/builds/push-images/).

## Deploy the application in Kubernetes

Now that the image is in the container registry, it is time to deploy it to Kubernetes. Create a Kubernetes deployment file. Reference [deployment.yaml](./k8s/deployment.yaml) as an example.

Some key lines:

```yaml
apiVersion: extensions/v1beta1
kind: Deployment
metadata:
  name: k8s-example
spec:
  replicas: 2
  template:
    # cut for short
    spec:
      containers:
        - name: k8s-web
          # Please update to use your own image in the container registry.
          image: dockeraccount/ai-k8s-app:0.0.1
          ports:
            - containerPort: 80
          env:
          # Uncomment the following lines to overwrite the Applicaiton Insights Instrumentation Key
          #- name: APPINSIGHTS_INSTRUMENTATIONKEY
          #  value: APPLICATION INSIGHTS KEY OVERWRITE 
          - name: ASPNETCORE_HOSTINGSTARTUPASSEMBLIES
            value: Microsoft.ApplicationInsights.Kubernetes.HostingStartup
```

**Note:** There is yet another chance to overwrite the environment variable of the application insights key. [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/) are recommended to protect the key in the production environment.

Then run the following kubectl command to deploy the app:

```bash
# kubectl create -f deployment.yaml
```

To learn more about Kubernetes deployment, please refer to [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/).

Generate some traffic to your application. After a while (around 2 minutes), you shall see Application Insights data coming with Kubernetes properties on it.

![Application Insights Events with Kubernetes Properties](./.media/AI_K8s_Properties.png)

## Summary

In this example, we modified the Dockerfile a bit to add the required NuGet packages into the project, then we used the environment variables to enable the feature.
The environment variable could also be managed by Kubernetes in the deployment spec.

Please also consider to protect the application insights instrumentation key by using various meanings, for example, by using the [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/).
