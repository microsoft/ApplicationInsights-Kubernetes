namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

/// <summary>
/// A service that tries to get container id.
/// </summary>
internal interface IContainerIdProvider
{
    /// <summary>
    /// Tries to get the container id.
    /// </summary>
    /// <returns>True with value when attempt successfully. False with null when failed.</returns>
    /// <remarks>
    /// It is possible to have true return with containerId == string.Empty. A known case is in Windows Container where
    /// there's no good way to get the container id from within the container.
    /// </remarks>
    bool TryGetMyContainerId(out string? containerId);
}
