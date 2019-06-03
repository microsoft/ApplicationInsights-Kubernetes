using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

#if !NETSTANDARD1_3 && !NETSTANDARD1_6
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

        public KubernetesTelemetryInitializer(
            IK8sEnvironmentFactory k8sEnvFactory,
            IOptions<AppInsightsForKubernetesOptions> options,
            SDKVersionUtils sdkVersionUtils)
        {
            _k8sEnvironment = null;

            // Options can't be null.
            Debug.Assert(options != null, "Options can't be null.");
            _options = Arguments.IsNotNull(options?.Value, nameof(options));

            _logger.LogDebug(@"Initialize Application Insights for Kubernetes telemetry initializer with Options:
{0}", JsonConvert.SerializeObject(_options));

            _sdkVersionUtils = Arguments.IsNotNull(sdkVersionUtils, nameof(sdkVersionUtils));
            _timeoutAt = DateTime.Now.Add(_options.InitializationTimeout);
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
                if (_isK8sQueryTimeout)
                {
                    if (!_isK8sQueryTimeoutReported)
                    {
                        _isK8sQueryTimeoutReported = true;
                        _logger.LogError("Query Kubernetes Environment timeout.");
                    }
                }
            }

            telemetry.Context.GetInternalContext().SdkVersion = _sdkVersionUtils.CurrentSDKVersion;
        }

        private async Task SetK8sEnvironment()
        {
            Task<IK8sEnvironment> createK8sEnvTask = _k8sEnvFactory.CreateAsync(_timeoutAt);
            await Task.WhenAny(
                createK8sEnvTask,
                Task.Delay(_timeoutAt - DateTime.Now)).ConfigureAwait(false);

            if (createK8sEnvTask.IsCompleted)
            {
                _k8sEnvironment = createK8sEnvTask.Result;
            }
            else
            {
                _isK8sQueryTimeout = true;
                _k8sEnvironment = null;
            }
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

            // Pod will have no replica name or deployment when deployed through other means. For example, as a daemonset.
            // Replica Set
            SetCustomDimension(telemetry, Invariant($"{K8s}.{ReplicaSet}.Name"), this._k8sEnvironment.ReplicaSetName, isValueOptional: true);

            // Deployment
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Deployment}.Name"), this._k8sEnvironment.DeploymentName, isValueOptional: true);

            // Node
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
                // A very simple but not that accurate evaluation of how much CPU the process is take out of a core.
                double CPUPercentage = (endCPUTime - startCPUTime).TotalMilliseconds / (double)(cpuWatch.ElapsedMilliseconds);
                cpuString = CPUPercentage.ToString("P2", CultureInfo.InvariantCulture);
            }

            long memoryInBytes = process.VirtualMemorySize64;

            SetCustomDimension(telemetry, Invariant($"{ProcessString}.{CPU}(%)"), cpuString);
            SetCustomDimension(telemetry, Invariant($"{ProcessString}.{Memory}"), memoryInBytes.GetReadableSize());
        }
#endif

        private static void SetCustomDimension(ITelemetry telemetry, string key, string value, bool isValueOptional = false)
        {
            if (telemetry == null)
            {
                _logger.LogError("telemetry object is null in telemetry initializer.");
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("Key is required to set custom dimension.");
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                if (isValueOptional)
                {
                    _logger.LogTrace("Value is null or empty for key: {0}", key);
                }
                else
                {
                    _logger.LogError("Value is required to set custom dimension. Key: {0}", key);
                }
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
                    _logger.LogTrace("The telemetry already contains the property of {0} with the same value of {1}.", key, existingValue);
                }
                else
                {
                    telemetry.Context.Properties[key] = value;
                    _logger.LogDebug("The telemetry already contains the property of {0} with value {1}. The new value is: {2}", key, existingValue, value);
                }
            }
        }

        private readonly SDKVersionUtils _sdkVersionUtils;
        private readonly DateTime _timeoutAt;

        internal readonly IK8sEnvironmentFactory _k8sEnvFactory;
        internal IK8sEnvironment _k8sEnvironment { get; private set; }

        internal AppInsightsForKubernetesOptions _options { get; private set; }

        internal bool _isK8sQueryTimeout = false;
        private bool _isK8sQueryTimeoutReported = false;
        private static readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;
    }
}
