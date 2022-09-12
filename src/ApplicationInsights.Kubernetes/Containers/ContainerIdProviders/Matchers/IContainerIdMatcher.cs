namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

/// <summary>
/// Matches container id.
/// </summary>
internal interface IContainerIdMatcher
{
    /// <summary>
    /// Matches the container id.
    /// </summary>
    /// <param name="value">The value to match.</param>
    /// <param name="containerId">The container id when matched.</param>
    /// <returns>Returns true when matched. Otherwise, false.</returns>
    bool TryParseContainerId(string? value, out string containerId);
}
