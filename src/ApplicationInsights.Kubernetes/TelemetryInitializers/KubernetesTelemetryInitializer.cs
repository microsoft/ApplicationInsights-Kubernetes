using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.Logging;

using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

#if !NETSTANDARD1_3 && !NETSTANDARD1_6
using System.Diagnostics;
using System.Globalization;
#endif

namespace Microsoft.ApplicationInsights.Kubernetes
{

    /// <summary>
    /// Telemetry Initializer for K8s Environment
    /// </summary>
    internal class KubernetesTelemetryInitializer : ITelemetryInitializer
    {
        public const string Container = "Container";
        public const string Deployment = "Deployment";
        public const string K8s = "Kubernetes";
        public const string Node = "Node";
        public const string Pod = "Pod";
        public const string ReplicaSet = "ReplicaSet";
        public const string ProcessString = "Process";
        public const string CPU = "CPU";
        public const string Memory = "Memory";

        private readonly ILogger _logger;
        private readonly SDKVersionUtils _sdkVersionUtils;
        private readonly TimeSpan _timeout;
        private readonly DateTime _timeoutTime;
        internal IK8sEnvironment _k8sEnvironment { get; private set; }
        internal readonly IK8sEnvironmentFactory _k8sEnvFactory;

        public KubernetesTelemetryInitializer(
            IK8sEnvironmentFactory k8sEnvFactory,
            TimeSpan timeout,
            SDKVersionUtils sdkVersionUtils,
            ILogger<KubernetesTelemetryInitializer> logger)
        {
            _logger = logger;
            _sdkVersionUtils = Arguments.IsNotNull(sdkVersionUtils, nameof(sdkVersionUtils));
            _timeout = Arguments.IsNotNull(timeout, nameof(timeout));
            _timeoutTime = DateTime.Now.Add(_timeout);
            _k8sEnvFactory = Arguments.IsNotNull(k8sEnvFactory, nameof(k8sEnvFactory));

            var _forget = SetK8sEnvironment();
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (_k8sEnvironment != null)
            {
#if NETSTANDARD2_0
                Stopwatch cpuWatch = Stopwatch.StartNew();
                TimeSpan startCPUTime = Process.GetCurrentProcess().TotalProcessorTime;
#endif
                // Setting the container name to role name
                if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
                {
                    telemetry.Context.Cloud.RoleName = this._k8sEnvironment.ContainerName;
                }

#if NETSTANDARD2_0
                SetCustomDimensions(telemetry, cpuWatch, startCPUTime);
#else
                SetCustomDimensions(telemetry);
#endif
            }
            else
            {
                // Lose 5 seconds before reporting the error just in case there is a delay before calling k8sEnvFactory.CreateAsync().
                if (DateTime.Now > _timeoutTime.AddSeconds(5))
                {
                    _logger.LogError("K8s Environemnt is null.");
                }
            }

            telemetry.Context.GetInternalContext().SdkVersion = _sdkVersionUtils.CurrentSDKVersion;
        }

        private async Task SetK8sEnvironment()
        {
            _k8sEnvironment = await _k8sEnvFactory.CreateAsync(_timeout).ConfigureAwait(false);
        }

        private void SetCustomDimensions(ITelemetry telemetry)
        {
            // Adding pod name into custom dimension

            // Container
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Container}.ID"), this._k8sEnvironment.ContainerID);
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Container}.Name"), this._k8sEnvironment.ContainerName);

            // Pod
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Pod}.ID"), this._k8sEnvironment.PodID);
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Pod}.Name"), this._k8sEnvironment.PodName);
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Pod}.Labels"), this._k8sEnvironment.PodLabels);

            // Replica Set
            SetCustomDimension(telemetry, Invariant($"{K8s}.{ReplicaSet}.Name"), this._k8sEnvironment.ReplicaSetName);

            // Deployment
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Deployment}.Name"), this._k8sEnvironment.DeploymentName);

            // Ndoe
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Node}.ID"), this._k8sEnvironment.NodeUid);
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Node}.Name"), this._k8sEnvironment.NodeName);
        }

#if NETSTANDARD2_0
        private void SetCustomDimensions(ITelemetry telemetry, Stopwatch cpuWatch, TimeSpan startCPUTime)
        {
            SetCustomDimensions(telemetry);

            // Add CPU/Memory metrics to telemetry.
            Process process = Process.GetCurrentProcess();
            TimeSpan endCPUTime = process.TotalProcessorTime;
            cpuWatch.Stop();

            string cpuString = "NaN";
            if (cpuWatch.ElapsedMilliseconds > 0)
            {
                int processorCount = Environment.ProcessorCount;
                Debug.Assert(processorCount > 0, $"How could process count be {processorCount}?");
                // A very simple but not that accruate evaluation of how much CPU the process is take out of a core.
                double CPUPercentage = (endCPUTime - startCPUTime).TotalMilliseconds / (double)(cpuWatch.ElapsedMilliseconds);
                cpuString = CPUPercentage.ToString("P2", CultureInfo.InvariantCulture);
            }

            long memoryInBytes = process.VirtualMemorySize64;

            SetCustomDimension(telemetry, Invariant($"{ProcessString}.{CPU}(%)"), cpuString);
            SetCustomDimension(telemetry, Invariant($"{ProcessString}.{Memory}"), memoryInBytes.GetReadableSize());
        }
#endif

        private void SetCustomDimension(ITelemetry telemetry, string key, string value)
        {
            if (telemetry == null)
            {
                _logger.LogError("telemetry object is null in telememtry initializer.");
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("Key is required to set custom dimension.");
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                _logger.LogError(Invariant($"Value is required to set custom dimension. Key: {key}"));
                return;
            }

            if (!telemetry.Context.Properties.ContainsKey(key))
            {
                telemetry.Context.Properties.Add(key, value);
            }
            else
            {
                string existingValue = telemetry.Context.Properties[key];
                if (string.Equals(existingValue, value, System.StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogTrace(Invariant($"The telemetry already contains the property of {key} with the same value of {existingValue}."));
                }
                else
                {
                    telemetry.Context.Properties[key] = value;
                    _logger.LogDebug(Invariant($"The telemetry already contains the property of {key} with value {existingValue}. The new value is: {value}"));
                }
            }
        }
    }
}
