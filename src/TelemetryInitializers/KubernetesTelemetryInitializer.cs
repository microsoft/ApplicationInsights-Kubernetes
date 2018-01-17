namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

#if !NETSTANDARD1_3 && !NETSTANDARD1_6
    using System.Diagnostics;
    using System.Globalization;
#endif

    /// <summary>
    /// Telemetry Initializer for K8s Environment
    /// </summary>
    public class KubernetesTelemetryInitializer : ITelemetryInitializer
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

        private ILogger<KubernetesTelemetryInitializer> logger;
        internal IK8sEnvironment K8sEnvironment { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public KubernetesTelemetryInitializer()
            : this(null, null)
        { }

#pragma warning disable CA2222 // Do not decrease inherited member visibility
        internal KubernetesTelemetryInitializer(
#pragma warning restore CA2222 // Do not decrease inherited member visibility
            ILoggerFactory loggerFactory,
            IK8sEnvironment env)
        {
            this.logger = loggerFactory?.CreateLogger<KubernetesTelemetryInitializer>();
            this.K8sEnvironment = env ?? Kubernetes.K8sEnvironment.CreateAsync(
                TimeSpan.FromMinutes(2), loggerFactory).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (K8sEnvironment != null)
            {
                // Setting the container name to role name
                if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
                {
                    telemetry.Context.Cloud.RoleName = this.K8sEnvironment.ContainerName;
                }

                SetCustomDimensions(telemetry);

                logger?.LogTrace(JsonConvert.SerializeObject(telemetry));
            }
            else
            {
                logger?.LogError("K8s Environemnt is null.");
            }
        }

        private void SetCustomDimensions(ITelemetry telemetry)
        {
            // Adding pod name into custom dimension

            // Container
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Container}.ID"), this.K8sEnvironment.ContainerID);
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Container}.Name"), this.K8sEnvironment.ContainerName);

            // Pod
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Pod}.ID"), this.K8sEnvironment.PodID);
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Pod}.Name"), this.K8sEnvironment.PodName);
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Pod}.Labels"), this.K8sEnvironment.PodLabels);

            // Replica Set
            SetCustomDimension(telemetry, Invariant($"{K8s}.{ReplicaSet}.Name"), this.K8sEnvironment.ReplicaSetName);

            // Deployment
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Deployment}.Name"), this.K8sEnvironment.DeploymentName);

            // Ndoe
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Node}.ID"), this.K8sEnvironment.NodeUid);
            SetCustomDimension(telemetry, Invariant($"{K8s}.{Node}.Name"), this.K8sEnvironment.NodeName);

#if !NETSTANDARD1_3 && !NETSTANDARD1_6
            // Add CPU/Memory metrics to telemetry.
            Process process = Process.GetCurrentProcess();
            TimeSpan cpuTimeSpan = process.TotalProcessorTime;
            long memoryInBytes = process.WorkingSet64;

            SetCustomDimension(telemetry, Invariant($"{ProcessString}.{CPU}(ms)"), cpuTimeSpan.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            SetCustomDimension(telemetry, Invariant($"{ProcessString}.{Memory}"), memoryInBytes.GetReadableSize());
#endif

        }

        private void SetCustomDimension(ITelemetry telemetry, string key, string value)
        {
            if (telemetry == null)
            {
                logger?.LogError("telemetry object is null in telememtry initializer.");
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                logger?.LogError("Key is required to set custom dimension.");
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                logger?.LogError(Invariant($"Value is required to set custom dimension. Key: {key}"));
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
                    logger?.LogDebug(Invariant($"The telemetry already contains the property of {key} with the same value of {existingValue}."));
                }
                else
                {
                    logger?.LogWarning(Invariant($"The telemetry already contains the property of {key} with value {existingValue}. The new value is: {value}"));
                }
            }
        }
    }
}
