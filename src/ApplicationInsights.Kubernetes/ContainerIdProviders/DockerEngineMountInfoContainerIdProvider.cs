#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

internal class DockerEngineMountInfoContainerIdProvider : FileBasedContainerIdProvider
{
    private const string InfoFilePath = "/proc/self/mountinfo";

    public DockerEngineMountInfoContainerIdProvider(
        DockerEngineMountInfoMatcher matcher,
        string filePath,
        string? providerName) : base(matcher, filePath, providerName)
    {
    }
}
