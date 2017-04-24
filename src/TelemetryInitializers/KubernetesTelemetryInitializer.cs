namespace Microsoft.ApplicationInsights.Kubernetes
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;

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
                telemetry.Context.Cloud.RoleName = this.K8sEnvironment.ContainerName;

                SetCustomDimensions(telemetry);
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
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Container}.ID"), this.K8sEnvironment.ContainerID);
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Container}.Name"), this.K8sEnvironment.ContainerName);

            // Pod
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Pod}.ID"), this.K8sEnvironment.PodID);
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Pod}.Name"), this.K8sEnvironment.PodName);
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Pod}.Labels"), this.K8sEnvironment.PodLabels);

            // Replica Set
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{ReplicaSet}.ID"), this.K8sEnvironment.ReplicaSetUid);

            // Deployment
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Deployment}.ID"), this.K8sEnvironment.DeploymentUid);

            // Ndoe
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Node}.ID"), this.K8sEnvironment.NodeUid);
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Node}.Name"), this.K8sEnvironment.NodeName);
        }
    }
}
