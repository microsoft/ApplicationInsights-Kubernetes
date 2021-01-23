using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class TelemetryKeyCache : ITelemetryKeyCache
    {
        // The capacity of 40 is kind of arbitrary. We have around 10 keys appended to for the telemetry;
        // Times that by 4 to allow a bit of wiggling room for name mapping;
        // However, having too many of the mapped key is not allowed to prevent huge memory consuption.
        internal const int CacheCapacity = 40;
        private readonly AppInsightsForKubernetesOptions _options;

        private ConcurrentDictionary<string, string> _telemetryKeyCache;

        public TelemetryKeyCache(IOptions<AppInsightsForKubernetesOptions> options)
        {
            _options = options?.Value ?? throw new System.ArgumentNullException(nameof(options));

            if (_options.TelemetryKeyProcessor != null)
            {
                _telemetryKeyCache = new ConcurrentDictionary<string, string>(
                    concurrencyLevel: 16,
                    // Set initial capacity same as the maximum for best performance.
                    capacity: CacheCapacity);
            }
        }

        public string GetProcessedKey(string originalKey)
        {
            // Implicit condition: telemetry cache stays null means the key processor is not set in options.
            if (_telemetryKeyCache == null)
            {
                return originalKey;
            }

            string result = _telemetryKeyCache.GetOrAdd(originalKey, _options.TelemetryKeyProcessor.Invoke(originalKey));

            if (_telemetryKeyCache.Count > CacheCapacity)
            {
                throw new InvalidOperationException($"Transformed key count is larger than the capacity of {CacheCapacity}. This is not allowed. Please verify the TelemetryKeyProcessor option.");
            }

            return result;
        }
    }
}