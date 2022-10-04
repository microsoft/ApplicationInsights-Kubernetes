using System;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.Containers;

internal class DockerEngineMountInfoMatcher : IContainerIdMatcher
{
    private const string LogCategory = nameof(DockerEngineMountInfoMatcher);
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    private const string MatchPattern = @"/docker/containers/(.*?)/";
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
            _logger.LogTrace($"[{LogCategory}] No match for containerId. Input: {line}, pattern: {MatchPattern}");
            return false;
        }
        _logger.LogTrace($"[{LogCategory}] Matched container id.");
        containerId = match.Groups[1].Value;
        return true;
    }
}
