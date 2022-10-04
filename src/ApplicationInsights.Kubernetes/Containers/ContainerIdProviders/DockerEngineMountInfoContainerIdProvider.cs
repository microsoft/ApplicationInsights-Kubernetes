namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

internal class DockerEngineMountInfoContainerIdProvider : FileBasedContainerIdProvider
{
    private const string InfoFilePath = "/proc/self/mountinfo";

    public DockerEngineMountInfoContainerIdProvider(
        DockerEngineMountInfoMatcher matcher)
            : base(matcher, InfoFilePath, providerName: default)
    {
    }
}
