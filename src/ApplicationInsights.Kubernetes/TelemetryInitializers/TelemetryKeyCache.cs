using System;
using System.Collections.Concurrent;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class TelemetryKeyCache : ITelemetryKeyCache
    {
        private readonly AppInsightsForKubernetesOptions _options;
        private readonly ConcurrentDictionary<string, string>? _telemetryKeyCache = null;
        private static readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;


        public TelemetryKeyCache(IOptions<AppInsightsForKubernetesOptions> options)
        {
            _options = options?.Value ?? throw new System.ArgumentNullException(nameof(options));

            if (_options.TelemetryKeyProcessor is not null)
            {
                _telemetryKeyCache = new ConcurrentDictionary<string, string>();
            }
        }

        public string GetProcessedKey(string originalKey)
        {
            Func<string, string>? telemetryKeyProcessor = _options.TelemetryKeyProcessor;
            if (telemetryKeyProcessor is null)
            {
                return originalKey;
            }

            if (_telemetryKeyCache is null)
            {
                _logger.LogWarning($"Why {nameof(telemetryKeyProcessor)} is not null while {nameof(_telemetryKeyCache)} is null? This should never happen, please file a bug at https://github.com/microsoft/ApplicationInsights-Kubernetes/issues/new. Fallback to use the original key.");
                return originalKey;
            }

            return _telemetryKeyCache.GetOrAdd(originalKey, valueFactory: _options.TelemetryKeyProcessor);
        }
    }
}
