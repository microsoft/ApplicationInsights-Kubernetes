#if NETSTANDARD2_0

using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using static Microsoft.ApplicationInsights.Kubernetes.TelemetryProperty;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal sealed class SimplePerformanceCounterTelemetryInitializer : ITelemetryInitializer
    {
        private TimeSpan _lastCPUSample;
        private Stopwatch _cpuWatch = new Stopwatch();

        // Cache current process for performance trade off. 
        // Refer to https://stackoverflow.com/questions/28023490/should-one-call-dispose-for-process-getcurrentprocess
        // for the reason that we can get away without calling dispose on it, thus not to implement the IDisposable interface.
        private Process _currnetProcess;

        private static readonly int ProcessorCount = Environment.ProcessorCount <= 0 ? 1 : Environment.ProcessorCount;
        private readonly ITelemetryKeyCache _telemetryKeyCache;

        public SimplePerformanceCounterTelemetryInitializer(ITelemetryKeyCache telemetryKeyCache)
        {
            _telemetryKeyCache = telemetryKeyCache ?? throw new ArgumentNullException(nameof(telemetryKeyCache));
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is ISupportMetrics metricsTelemetry)
            {
                // Write CPU metrics
                TimeSpan endCPUTime = GetCPUTime();
                if (_lastCPUSample != default)
                {
                    if (_cpuWatch.ElapsedMilliseconds > 0)
                    {
                        // A very simple but not that accurate evaluation of how much CPU the process is take out of a core.
                        double cpuPercentage = (endCPUTime - _lastCPUSample).TotalMilliseconds / (_cpuWatch.ElapsedMilliseconds) / ProcessorCount;
                        SetMetrics(metricsTelemetry, ProcessProperty.CPUPrecent, cpuPercentage);
                    }
                    else
                    {
                        _cpuWatch.Start();
                    }
                }

                // Snap CPU usage
                _lastCPUSample = endCPUTime;
                _cpuWatch.Restart();

                // Write memory metrics
                long memory = GetMemory();
                SetMetrics(metricsTelemetry, ProcessProperty.Memory, memory);
            }
        }

        private void SetMetrics(ISupportMetrics telemetry, string key, double value)
        {
            key = _telemetryKeyCache.GetTelemetryProcessedKey(key);
            if (!telemetry.Metrics.ContainsKey(key))
            {
                telemetry.Metrics.Add(key, value);
            }
        }

        private TimeSpan GetCPUTime() => GetFromCurrentProcess<TimeSpan>(p => p.TotalProcessorTime);
        private long GetMemory() => GetFromCurrentProcess<long>(p => p.VirtualMemorySize64);

        private T GetFromCurrentProcess<T>(Func<Process, T> valueFactory)
        {
            // In case of concurrent execution, there is a chance for _currentProcess to be checked null more than one time,
            // Take the hit to assign it multiple times to avoid locks.
            if (_currnetProcess == null)
            {
                _currnetProcess = Process.GetCurrentProcess();
            }
            return valueFactory(_currnetProcess);
        }
    }
}
#endif