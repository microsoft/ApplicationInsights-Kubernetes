# Configure RBAC permissions

`Microsoft.ApplicationInsights.Kubernetes` uses the service account to query kubernetes information to enhance telemetries. It is important to have proper permissions configured for kubernetes related information like node, pod and so on to be fetched correctly.

In this post, we will start by describe a method to correctly configure the permissions for an RBAC enabled cluster. And then share a guidance for troubleshooting.

## Assumptions

In this demo, we will have the following assumptions. Please change the related values accordingly:

* The application will be deployed to namespace of `ai-k8s-demo`.
* The application will leverage the `default` service account.

## Configure ClusterRole and ClusterRoleBinding for the service account

* Create a yaml file, [sa-role.yaml](./sa-role.yaml) for example. We will deploy it when it is ready.

    * Write spec to define a cluster role, name it `appinsights-k8s-property-reader` for example:
 
        ```yaml
        kind: ClusterRole
        apiVersion: rbac.authorization.k8s.io/v1
        metadata:
        # "namespace" omitted since ClusterRoles are not namespaced
        name: appinsights-k8s-property-reader
        rules:
        - apiGroups: ["", "apps"]
        resources: ["pods", "nodes", "replicasets", "deployments"]
        verbs: ["get", "list"]
        ```
        That spec defines the name of the role, and what permission does the role has, for example, list pods.

        You don't have to use the exact name, but you will need to making sure the name is referenced correctly in the following steps.

    * Append a Cluster role binding spec:

        ```yaml
        ---
        kind: ClusterRoleBinding
        apiVersion: rbac.authorization.k8s.io/v1
        metadata:
        name: appinsights-k8s-property-reader-binding
        subjects:
        - kind: ServiceAccount
        name: default
        namespace: 
        roleRef:
        kind: ClusterRole
        name: appinsights-k8s-property-reader
        apiGroup: rbac.authorization.k8s.io
        ```
    
    That is to grant the role of `appinsights-k8s-property-reader` to the default service account in namespace of `ai-k8s-demo`.
    
* Now you can deploy it:

    ```shell
    kubectl create -f `sa-role.yaml`
    ```
    See [sa-role.yaml](sa-role.yaml) for a full example.

> :warning: Check back for various permissions needed. Depends on the implementations, it may change in the over time.

## Ad-hoc troubleshooting for permission

Kubectl provides an `auth --can-i` sub command for troubleshooting permissions. It supports impersonate the service account. We can leverage it for permission troubleshooting, for example:

```shell
kubectl auth can-i list pod --namespace ai-k8s-demo --as system:serviceaccount:ai-k8s-demo:default
```

There are several arguments there, let's break it down:

* **--namespace ai-k8s-demo**: This is the target namespace;
* **--as system:serviceaccount:ai-k8s-demo:default**: Check permission for `default` service account in namespace of `ai-k8s-demo`.

The response for the command-line will simply be `yes` or `no`, for example:

```shell
$ kubectl auth can-i list pod --namespace ai-k8s-demo --as system:serviceaccount:ai-k8s-demo:default
yes
```

## Use SubjectAccessReview

Kubernetes also provides `SubjectAccessReview` to check permission for given user on target resource.

### Basic usage

* Create a yaml file like `subject-access-review.yaml` for example:

    ```yaml
    apiVersion: authorization.k8s.io/v1
    kind: SubjectAccessReview
    spec:
    resourceAttributes:
        group: "apps"
        resource: pods
        verb: list
        namespace: ai-k8s-demo
    user: system:serviceaccount:ai-k8s-demo:default # service account in default namespace and named default
    ```
* Deploy and output the content as yaml:

    ```shell
    kubectl create -f subject-access-review.yaml -o yaml
    ```
    When the permission is configured correctly, it will give out detailed descriptions like this:

    ```yaml
    apiVersion: authorization.k8s.io/v1
    kind: SubjectAccessReview
    metadata:
    creationTimestamp: null
    spec:
    resourceAttributes:
        namespace: ai-k8s-demo
        resource: pods
        verb: list
    user: system:serviceaccount:ai-k8s-demo:default
    status:
    allowed: true
    reason: 'RBAC: allowed by ClusterRoleBinding "appinsights-k8s-property-reader-binding"
        of ClusterRole "appinsights-k8s-property-reader" to ServiceAccount "ai-k8s-demo/default"'
    ```

    Or, lack of permission:
    
    ```yaml
    apiVersion: authorization.k8s.io/v1
    kind: SubjectAccessReview
    metadata:
    creationTimestamp: null
    spec:
    resourceAttributes:
        namespace: ai-k8s-demo
        resource: pods
        verb: list
    user: system:serviceaccount:ai-k8s-demo:default
    status:
    allowed: false
    ```

    To make it easier, we provide 2 pre-built yaml for you to leverage:

    * [subject-access-review-key.yaml](./subject-access-review-key.yaml): a subset of permissions for probing the RBAC settings.
    * [subject-access-review-full.yaml](./subject-access-review-full.yaml): a full list of all permissions needed for RBAC settings in case any specific permission is missing.

Let us know if there's questions, suggestions by filing [issues](https://github.com/microsoft/ApplicationInsights-Kubernetes/issues).

## References

* [Checking API Access](https://kubernetes.io/docs/reference/access-authn-authz/authorization/#checking-api-access)
