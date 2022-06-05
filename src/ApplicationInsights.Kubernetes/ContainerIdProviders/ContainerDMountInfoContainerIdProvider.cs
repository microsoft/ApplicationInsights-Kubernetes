#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

internal class ContainerDMountInfoContainerIdProvider : FileBasedContainerIdProvider
{
    private const string InfoFilePath = "/proc/self/mountinfo";

    public ContainerDMountInfoContainerIdProvider(
        ContainerDMountInfoMatcher matcher,
        IStreamLineReader streamLineReader)
            : base(matcher, streamLineReader, InfoFilePath, providerName: default)
    {
    }
}
