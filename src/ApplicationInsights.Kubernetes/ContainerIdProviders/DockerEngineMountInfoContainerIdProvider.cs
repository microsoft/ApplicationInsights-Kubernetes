#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

internal class DockerEngineMountInfoContainerIdProvider : FileBasedContainerIdProvider
{
    private const string InfoFilePath = "/proc/self/mountinfo";

    public DockerEngineMountInfoContainerIdProvider(
        DockerEngineMountInfoMatcher matcher,
        IStreamLineReader streamLineReader)
            : base(matcher, streamLineReader, InfoFilePath, providerName: default)
    {
    }
}
