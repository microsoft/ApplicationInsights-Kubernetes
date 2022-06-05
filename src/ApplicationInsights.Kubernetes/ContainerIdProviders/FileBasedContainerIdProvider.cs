#nullable enable

using System.IO;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

/// <summary>
/// A common framework to get container id from a file, providing consistent implementation as well as logging.
/// </summary>
internal abstract class FileBasedContainerIdProvider : IContainerIdProvider
{
    private readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    private readonly IContainerIdMatcher _lineMatcher;
    private readonly string _providerName;
    private readonly string _targetFile;

    public FileBasedContainerIdProvider(
        IContainerIdMatcher lineMatcher,
        string filePath, 
        string? providerName)
    {
        _providerName = this.GetType().Name;

        if (string.IsNullOrEmpty(filePath))
        {
            throw new System.ArgumentException($"'{nameof(filePath)}' cannot be null or empty.", nameof(filePath));
        }
        _targetFile = filePath;

        _lineMatcher = lineMatcher ?? throw new System.ArgumentNullException(nameof(lineMatcher));
    }

    public bool TryGetMyContainerId(out string? containerId)
    {
        containerId = FetchContainerId(_targetFile);
        return containerId != null;
    }

    private string? FetchContainerId(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"[{_providerName}] {nameof(_targetFile)} doesn't exist at: {filePath}");
            return null;
        }

        using StreamReader reader = File.OpenText(_targetFile);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if(string.IsNullOrEmpty(line))
            {
                continue;
            }
            
            if (_lineMatcher.TryParseContainerId(line, out string containerId))
            {
                _logger.LogDebug($"[{_providerName}] Got container id by: {line}");
                _logger.LogInformation($"[{_providerName}]Got container id: {containerId}");
                return containerId;
            }
        }
        _logger.LogWarning($"[{_providerName}]Can't figure out container id.");
        return null;
    }
}
