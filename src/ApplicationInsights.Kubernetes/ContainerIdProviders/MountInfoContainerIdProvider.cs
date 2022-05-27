#nullable enable

using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

internal class MountInfoContainerIdProvider : IContainerIdProvider
{
    private const string InfoFilePath = "/proc/self/mountinfo";
    private const string MatchPattern = @"\/docker\/containers\/(.*?)\/";
    private static readonly Regex MatchRegex = new Regex(MatchPattern, RegexOptions.CultureInvariant | RegexOptions.Multiline, TimeSpan.FromSeconds(1));

    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    public bool TryGetMyContainerId(out string? containerId)
    {
        containerId = FetchContainerId(InfoFilePath);
        return containerId != null;
    }

    private string? FetchContainerId(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"{nameof(InfoFilePath)} file doesn't exist. Path: {filePath}");
            return null;
        }

        using StreamReader reader = File.OpenText(InfoFilePath);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if (TryParseContainerId(line, out string containerId))
            {
                return containerId;
            }
        }
        _logger.LogWarning("Can't figure out container id.");
        return null;
    }

    internal bool TryParseContainerId(string? line, out string containerId)
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

        containerId = match.Groups[1].Value;
        return true;
    }
}
