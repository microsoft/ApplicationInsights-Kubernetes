namespace Microsoft.ApplicationInsights.Kubernetes.Pods;

internal class HostNamePodNameProvider : EnvironmentVariablePodNameProviderBase
{
    internal const string VariableName = "HOSTNAME";
    public HostNamePodNameProvider() : base(VariableName)
    {
    }
}
