#nullable enable

using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders
{
    /// <summary>
    /// Gets the current container id by using CGroup
    /// </summary>
    internal class CGroupContainerIdProvider : IContainerIdProvider
    {
        private const string CGroupPath = "/proc/self/cgroup";
        private const string CGroupPathPatternString = "cpu.+/([^/]*)$";
        private static readonly Regex CGroupPathPattern = new Regex(CGroupPathPatternString, RegexOptions.CultureInvariant | RegexOptions.Multiline, TimeSpan.FromSeconds(1));

        private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        public bool TryGetMyContainerId(out string? containerId)
        {
            containerId = FetchContainerId(CGroupPath);
            return containerId != null;
        }

        private string? FetchContainerId(string pathToCGroup)
        {
            if (!File.Exists(pathToCGroup))
            {
                _logger.LogWarning("CGroup file doesn't exist. Path: {0}", pathToCGroup);
                return null;
            }

            string content = File.ReadAllText(pathToCGroup);
            return ParseContainerId(content);
        }

        internal string? ParseContainerId(string? content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                MatchCollection matches = CGroupPathPattern.Matches(content);
                if (matches.Count >= 1 && matches[0].Groups.Count >= 2)
                {
                    string containerId = matches[0].Groups[1].Value;
                    _logger.LogInformation($"Got container id: {containerId}");
                    return containerId;
                }
            }
            _logger.LogWarning("Can't figure out container id. Input: {0}. Pattern: {1}", content, CGroupPathPatternString);
            return null;
        }
    }
}
