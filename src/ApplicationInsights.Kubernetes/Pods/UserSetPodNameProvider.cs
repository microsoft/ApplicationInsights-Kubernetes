namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

internal class UserSetPodNameProvider : EnvironmentVariablePodNameProviderBase
{
    public UserSetPodNameProvider() : base("APPINSIGHTS_KUBERNETES_POD_NAME")
    {
    }
}
