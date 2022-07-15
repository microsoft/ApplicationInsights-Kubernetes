#nullable enable

using System;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

/// <summary>
/// Ths is a heuristic matcher for container id in containers by containerD using the mount info.
/// More info about MountInfo: https://man7.org/linux/man-pages/man5/proc.5.html
/// </summary>
internal class ContainerDMountInfoMatcher : IContainerIdMatcher
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

    // An example of container id line:
    // 1735 1729 0:37 /kubepods/besteffort/pod3272f253-be44-4a82-a541-9083e68cf99f/a22b3a93bd510bf062765ec5df6608fa6cae186a476b0407bfb5369ff99afdd2 /sys/fs/cgroup/hugetlb ro,nosuid,nodev,noexec,relatime master:19 - cgroup cgroup rw,hugetlb
    // See unit tests for more examples.
    // This is heuristic, the mount path is not always guaranteed. File issue at https://github.com/microsoft/ApplicationInsights-Kubernetes/issues if/when find it changed.
    private const string MatchPattern = @"/kubepods/.*?/.*?/(.*?)[\s|/]";
    private static readonly Regex MatchRegex = new Regex(MatchPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

    private const string LogCategory = nameof(ContainerDMountInfoMatcher);

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
