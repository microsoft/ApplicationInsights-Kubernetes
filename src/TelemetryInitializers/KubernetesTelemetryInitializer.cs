namespace Microsoft.ApplicationInsights.Kubernetes
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;
    using static Microsoft.ApplicationInsights.Kubernetes.TelemetryInitializers.Prefixes;

    /// <summary>
    /// Telemetry Initializer for K8s Environment
    /// </summary>
    internal class KubernetesTelemetryInitializer : ITelemetryInitializer
    {
        private ILogger<KubernetesTelemetryInitializer> logger;
        internal IK8sEnvironment K8sEnvironment { get; private set; }

        public KubernetesTelemetryInitializer(
            ILoggerFactory loggerFactory,
            IK8sEnvironment env)
        {
            this.logger = loggerFactory?.CreateLogger<KubernetesTelemetryInitializer>();
            this.K8sEnvironment = env;
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
