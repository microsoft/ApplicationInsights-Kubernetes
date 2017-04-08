namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    using static Microsoft.ApplicationInsights.Netcore.Kubernetes.StringUtils;
    using static Microsoft.ApplicationInsights.Netcore.Kubernetes.TelemetryInitializers.Prefixes;

    /// <summary>
    /// Telemetry Initializer for K8s Environment
    /// </summary>
    public class KubernetesTelemetryInitializer : ITelemetryInitializer
    {
        K8sEnvironment k8sEnvironment;
        public KubernetesTelemetryInitializer(K8sEnvironment env)
        {
            this.k8sEnvironment = env;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (k8sEnvironment != null)
            {
                // Setting the container name to role name
                telemetry.Context.Cloud.RoleName = this.k8sEnvironment.ContainerName;

                SetCustomDimensions(telemetry);
            }
            else
            {
                // TODO: Use event listner instead of console output for self-diagnostics.
                System.Console.WriteLine("K8s Environemnt is null.");
            }
        }

        private void SetCustomDimensions(ITelemetry telemetry)
        {
            // Adding pod name into custom dimension

            // Container
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Container}.ID"), this.k8sEnvironment.ContainerID);
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Container}.Name"), this.k8sEnvironment.ContainerName);

            // Pod
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Pod}.Name"), this.k8sEnvironment.PodName);
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Pod}.ID"), this.k8sEnvironment.PodID);
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Pod}.Labels"), this.k8sEnvironment.PodLabels);

            // Replica Set
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{ReplicaSet}.ID"), this.k8sEnvironment.ReplicaSetUid);

            // Deployment
            telemetry.Context.Properties.Add(Invariant($"{K8s}.{Deployment}.ID"), this.k8sEnvironment.DeploymentUid);
        }
    }
}
