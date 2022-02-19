#nullable enable

using Microsoft.Extensions.Logging;
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

        private readonly ILogger<CGroupContainerIdProvider> _logger;

        public CGroupContainerIdProvider(ILogger<CGroupContainerIdProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool TryGetMyContainerId(out string? containerId)
        {
            containerId = FetchContainerId(CGroupPath);
            return containerId != null;
        }

        private string? FetchContainerId(string pathToCGroup)
        {
            if (!File.Exists(pathToCGroup))
            {
                _logger.LogWarning("CGroup file doesn't exist. Path: {filePath}", pathToCGroup);
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
                    return matches[0].Groups[1].Value;
                }
            }
            _logger.LogWarning("Can't figure out container id. Input: {content}. Pattern: {CGroupPathPatternString}", content, CGroupPathPatternString);
            return null;
        }
    }
}
