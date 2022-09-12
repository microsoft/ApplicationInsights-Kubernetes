namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

internal class ContainerDMountInfoContainerIdProvider : FileBasedContainerIdProvider
{
    private const string InfoFilePath = "/proc/self/mountinfo";

    public ContainerDMountInfoContainerIdProvider(
        ContainerDMountInfoMatcher matcher)
            : base(matcher, InfoFilePath, providerName: default)
    {
    }
}
