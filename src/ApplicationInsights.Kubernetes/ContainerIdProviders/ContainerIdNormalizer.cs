#nullable enable

using System;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

/// <summary>
/// A simple container id normalizer that picks out 64 digits of GUID/UUID from a container id with prefix / suffix.
/// For example:
/// cri-containerd-5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06.scope will be normalized to
/// 5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06
/// </summary>
internal class ContainerIdNormalizer : IContainerIdNormalizer
{
    // Simple rule: 64-characters GUID/UUID.
    private const string ContainerIdIdentifierPattern = @"[a-f0-9]{64}";
    private readonly Regex _matcher = new Regex(ContainerIdIdentifierPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeout: TimeSpan.FromSeconds(1));
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;


    public bool TryNormalize(string input, out string? normalized)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException($"'{nameof(input)}' cannot be null or empty.", nameof(input));
        }

        _logger.LogDebug($"Normalize container id: {input}");

        Match match = _matcher.Match(input);
        if (!match.Success)
        {
            _logger.LogDebug($"Failed match any container id by pattern: {ContainerIdIdentifierPattern}.");
            normalized = null;
            return false;
        }
        normalized = match.Value;
        _logger.LogTrace($"Container id normalized to: {normalized}");
        return true;
    }
}

