# Troubleshooting the Environment
Kubernetes cluster configuration could be complex. To run the app with `Application Insights for Kubernetes` on does not always work on the first try. It is a challenge when it is not clear where the issue comes from - the configuration for Kubernetes, the build of the images or the library of `Application Insights for Kubernetes`.

The purpose of this project is to provide a known good image with `Application Insights for Kubernetes` pre-installed, that can be easily deployed to the K8s cluster to verify it runs. The image also takes the role to gather trace logs to identify the possible cause of the issues.

## Prerequisite
* A Kubernetes Cluster that you can manage with kubectl.
  * If you don't have any, an easy way is to go to [Azure AKS](https://docs.microsoft.com/en-us/azure/aks/) to get a managed cluster. Verify that the credential is properly set for kubectl to work:
    ```bash
    user@user-pc:~$ kubectl get nodes
    NAME                       STATUS    ROLES     AGE       VERSION
    aks-nodepool1-10984277-0   Ready     agent     17d       v1.9.9
    aks-nodepool1-10984277-1   Ready     agent     17d       v1.9.9
    aks-nodepool1-10984277-2   Ready     agent     17d       v1.9.9
    user@user-pc:~$
    ```
  * The latest Docker for Windows Community Edition also supports using Kubernetes as its orchestrator.

## Deploy the troubleshooting image
* Save [k8s.yaml](AIK8sTroubleShooting/k8s/k8s.yaml) to a local folder.
* Deploy it:
```
kubectl create -f k8s.yaml
```
* Wait until the pod becomes 'Running':
```
kubectl get pods
```
Result
```
NAME                                      READY     STATUS    RESTARTS   AGE
ai-k8s-troubleshooting-69fcc95796-v4gxn   1/1       Running   0          12s
```
* Get the log for the pods:
```
kubectl logs ai-k8s-troubleshooting-<random-string>
```
Result when your cluster is configured correctly:
```
trce: Microsoft.ApplicationInsights.Kubernetes.K8sQueryClient[0]
      Default Header: Authorization: Bearer <Your Bearer Token>
dbug: Microsoft.ApplicationInsights.Kubernetes.K8sQueryClient[0]
      Query succeeded.
```
If there are issues, you will see errors like this:
```
dbug: Microsoft.ApplicationInsights.Kubernetes.K8sQueryClient[0]
      Query Failed. Request Message: Method: GET, RequestUri: 'https://10.0.0.1/api/v1/namespaces/default/pods', Version: 2.0, Content: <null>, Headers:
      {
        Authorization: Bearer <Your Bearer Token>
      }. Status Code: Forbidden. Phase: Forbidden
crit: Microsoft.ApplicationInsights.Kubernetes.K8sEnvironmentFactory[0]
      Fail to fetch the pod information in time. Kubernetes info will not be available for the telemetry.
```

_Tips:_ after fixing the configuration, delete the pod directly, another pod will be created automatically so that you don't have to do the redeployment.

## Under the hood
If you are interested in how the troubleshooting image is built, reference the code in [AIK8sTroubleShooting](./AIK8sTroubleShooting) folder.

## Next step
Reference the [examples](../examples) to add `Application Insights for Kubernetes` to your projects.

