#if NETSTANDARD2_0

using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using static Microsoft.ApplicationInsights.Kubernetes.TelemetryProperty;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class SimplePerformanceCounterTelemetryInitializer : ITelemetryInitializer
    {
        TimeSpan _lastCPUSample;
        Stopwatch _cpuWatch = new Stopwatch();
        private static readonly int ProcessorCount = Environment.ProcessorCount <= 0 ? 1 : Environment.ProcessorCount;

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
            if (!telemetry.Metrics.ContainsKey(key))
            {
                telemetry.Metrics.Add(key, value);
            }
        }

        private TimeSpan GetCPUTime() => GetFromCurrentProcess<TimeSpan>(p => p.TotalProcessorTime);
        private long GetMemory() => GetFromCurrentProcess<long>(p => p.VirtualMemorySize64);

        private T GetFromCurrentProcess<T>(Func<Process, T> valueFactory)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                return valueFactory(currentProcess);
            }
        }
    }
}
#endif