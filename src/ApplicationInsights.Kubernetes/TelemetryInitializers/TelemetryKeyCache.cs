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
                _telemetryKeyCache = new ConcurrentDictionary<string, string>(
                    concurrencyLevel: 16,
                    capacity: 40);
            }
        }

        public string GetTelemetryProcessedKey(string originalKey)
        {
            // Implicit condition: telemetry cache stays null means the key processor is not set in options.
            if (_telemetryKeyCache == null)
            {
                return originalKey;
            }

            return _telemetryKeyCache.GetOrAdd(originalKey, _options.TelemetryKeyProcessor.Invoke(originalKey));
        }
    }
}