using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class TelemetryKeyCache : ITelemetryKeyCache
    {
        private readonly AppInsightsForKubernetesOptions _options;
        private readonly ConcurrentDictionary<string, string> _telemetryKeyCache = new ConcurrentDictionary<string, string>();

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

            return _telemetryKeyCache.GetOrAdd(originalKey, valueFactory: _options.TelemetryKeyProcessor);
        }
    }
}
