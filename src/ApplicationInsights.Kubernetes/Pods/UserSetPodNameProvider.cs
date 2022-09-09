namespace Microsoft.ApplicationInsights.Kubernetes.Pods;

internal class UserSetPodNameProvider : EnvironmentVariablePodNameProviderBase
{
    public UserSetPodNameProvider() : base("APPINSIGHTS_KUBERNETES_POD_NAME")
    {
    }
}
