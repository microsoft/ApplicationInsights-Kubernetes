#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

/// <summary>
/// Matches container id.
/// </summary>
public interface IContainerIdMatcher
{
    /// <summary>
    /// Matches the container id.
    /// </summary>
    /// <param name="value">The value to match.</param>
    /// <param name="containerId">The container id when matched.</param>
    /// <returns>Returns true when matched. Otherwise, false.</returns>
    bool TryParseContainerId(string? value, out string containerId);
}
