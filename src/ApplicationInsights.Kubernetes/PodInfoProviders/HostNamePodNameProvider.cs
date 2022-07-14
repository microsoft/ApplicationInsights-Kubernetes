namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

internal class HostNamePodNameProvider : EnvironmentVariablePodNameProviderBase
{
    internal const string VariableName = "HOSTNAME";
    public HostNamePodNameProvider() : base(VariableName)
    {
    }
}
