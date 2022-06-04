#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

internal class ContainerDMountInfoContainerIdProvider : FileBasedContainerIdProvider
{
    private const string InfoFilePath = "/proc/self/mountinfo";

    public ContainerDMountInfoContainerIdProvider(
        ContainerDMountInfoMatcher matcher,
        string filePath,
        string? providerName) : base(matcher, filePath, providerName)
    {
    }
}
