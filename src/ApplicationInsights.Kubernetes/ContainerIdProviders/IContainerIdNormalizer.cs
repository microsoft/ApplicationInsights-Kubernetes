#nullable enable

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

/// <summary>
/// A service to normalize container id.
/// </summary>
internal interface IContainerIdNormalizer
{
    /// <summary>
    /// Tries to normalize container id.
    /// </summary>
    /// <param name="input">The container id.</param>
    /// <param name="normalized">The normalized container id.</param>
    /// <returns>True when normalized. False otherwise.</returns>
    bool TryNormalize(string input, out string? normalized);
}
