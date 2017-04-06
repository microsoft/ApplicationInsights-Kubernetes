namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    using static Microsoft.ApplicationInsights.Netcore.Kubernetes.StringUtils;

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
            const string Prefix = "K8s.";
            // Adding pod name into custom dimension
            telemetry.Context.Properties.Add(Invariant($"{Prefix}PodName"), this.k8sEnvironment.PodName);
            telemetry.Context.Properties.Add(Invariant($"{Prefix}Labels"), this.k8sEnvironment.PodLabels);
        }
    }
}
