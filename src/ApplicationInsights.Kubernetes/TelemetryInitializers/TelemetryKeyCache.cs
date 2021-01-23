using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class TelemetryKeyCache : ITelemetryKeyCache
    {
        private readonly AppInsightsForKubernetesOptions _options;

        private ConcurrentDictionary<string, string> _telemetryKeyCache;

        public TelemetryKeyCache(IOptions<AppInsightsForKubernetesOptions> options)
        {
            _options = options?.Value ?? throw new System.ArgumentNullException(nameof(options));

            if (_options.TelemetryKeyProcessor != null)
            {
                _telemetryKeyCache = new ConcurrentDictionary<string, string>();
            }
        }

        public string GetProcessedKey(string originalKey)
        {
            if (_options.TelemetryKeyProcessor == null)
            {
                return originalKey;
            }
            return _telemetryKeyCache.GetOrAdd(originalKey, valueFactory: _options.TelemetryKeyProcessor);
        }
    }
}