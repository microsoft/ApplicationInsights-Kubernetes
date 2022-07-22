namespace Microsoft.ApplicationInsights.Kubernetes;

internal interface IContainerIdHolder
{
    string? ContainerId { get; }
}
