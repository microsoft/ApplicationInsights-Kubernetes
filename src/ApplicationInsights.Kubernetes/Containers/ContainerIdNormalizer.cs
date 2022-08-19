#nullable enable

using System;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

/// <summary>
/// A simple container id normalizer that picks out 64 digits of GUID/UUID from a container id with prefix / suffix.
/// For example:
/// cri-containerd-5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06.scope will be normalized to
/// 5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06
/// </summary>
internal class ContainerIdNormalizer : IContainerIdNormalizer
{
    // Simple rule: First 64-characters GUID/UUID.
    private const string ContainerIdIdentifierPattern = @"([a-f\d]{64})";
    private readonly Regex _matcher = new Regex(ContainerIdIdentifierPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeout: TimeSpan.FromSeconds(1));
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    /// <summary>
    /// Gets normalized container id.
    /// </summary>
    /// <param name="input">The original container id. String.Empty yields string.Empty with true. Null is not accepted.</param>
    /// <param name="normalized">The normalized container id.</param>
    /// <returns>True when the normalized succeeded. False otherwise.</returns>
    public bool TryNormalize(string input, out string? normalized)
    {
        // Should not happen. Put here just in case.
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        // Special case: string.Empty in, string.Empty out.
        if (input == string.Empty)
        {
            normalized = string.Empty;
            return true;
        }

        _logger.LogDebug($"Normalize container id: {input}");

        Match match = _matcher.Match(input);
        if (!match.Success)
        {
            _logger.LogDebug($"Failed match any container id by pattern: {ContainerIdIdentifierPattern.EscapeForLoggingMessage()}.");
            normalized = null;
            return false;
        }
        normalized = match.Groups[1].Value;
        _logger.LogTrace($"Container id normalized to: {normalized}");
        return true;
    }
}

