#nullable enable

using System;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

internal class CGroupV1Matcher : IContainerIdMatcher
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    private const string MatchPattern = @"cpu.+/([^/]*)$";
    private static readonly Regex MatchRegex = new Regex(MatchPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

    public bool TryParseContainerId(string? line, out string containerId)
    {
        containerId = string.Empty;
        if (string.IsNullOrEmpty(line))
        {
            return false;
        }

        Match match = MatchRegex.Match(line);
        if (!match.Success)
        {
            _logger.LogDebug($"No match for containerId. Input: {line}, pattern: {MatchPattern}");
            return false;
        }
        _logger.LogTrace($"Matched container id.");
        containerId = match.Groups[1].Value;
        return true;
    }
}
