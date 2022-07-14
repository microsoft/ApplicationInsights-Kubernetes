namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

internal class HostNamePodNameProvider : EnvironmentVariablePodNameProviderBase
{
    public HostNamePodNameProvider() : base("HOSTNAME")
    {
    }
}
