#if NETSTANDARD2_0
using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Microsoft.ApplicationInsights.Kubernetes.TelemetryProperty;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubernetesTelemetryInitializerExt : KubernetesTelemetryInitializer
    {
        public KubernetesTelemetryInitializerExt(
            IK8sEnvironmentFactory k8sEnvFactory,
            IOptions<AppInsightsForKubernetesOptions> options,
            SDKVersionUtils sdkVersionUtils) :
            base(k8sEnvFactory, options, sdkVersionUtils)
        {

            string initializerName = nameof(KubernetesTelemetryInitializerExt);
            Debug.Assert(!_options.DisableCounters, $"{initializerName} is not supposed to be used when performance counters are disabled.");
            _logger.LogDebug("{InitializerName} should not be used when performance counters are disabled.", initializerName);
        }

        protected override void InitializeTelemetry(ITelemetry telemetry, IK8sEnvironment k8sEnv)
        {
            Stopwatch cpuWatch = Stopwatch.StartNew();
            TimeSpan startCPUTime = GetCPUTime();

            // Setting the container name to role name
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                telemetry.Context.Cloud.RoleName = k8sEnv.ContainerName;
            }

            if (telemetry is ISupportProperties propertySetter)
            {
                SetCustomDimensions(propertySetter, cpuWatch, startCPUTime);
            }
            else
            {
                _logger.LogTrace("This telemetry object doesn't implement ISupportProperties.");
            }
            _logger.LogTrace("Finish telemetry initializer.");
        }

        private void SetCustomDimensions(ISupportProperties telemetry, Stopwatch cpuWatch, TimeSpan startCPUTime)
        {
            SetCustomDimensions(telemetry);
            // Add CPU/Memory metrics to telemetry.
            TimeSpan endCPUTime = GetCPUTime();
            cpuWatch.Stop();

            string cpuString = "NaN";
            if (cpuWatch.ElapsedMilliseconds > 0)
            {
                int processorCount = Environment.ProcessorCount;
                Debug.Assert(processorCount > 0, $"How could process count be {processorCount}?");
                // A very simple but not that accurate evaluation of how much CPU the process is take out of a core.
                double CPUPercentage = (endCPUTime - startCPUTime).TotalMilliseconds / (double)(cpuWatch.ElapsedMilliseconds);
                cpuString = CPUPercentage.ToString("P2", CultureInfo.InvariantCulture);
            }

            long memoryInBytes = GetMemory();

            SetCustomDimension(telemetry, ProcessProperty.CPUPrecent, cpuString);
            SetCustomDimension(telemetry, ProcessProperty.Memory, memoryInBytes.GetReadableSize());
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