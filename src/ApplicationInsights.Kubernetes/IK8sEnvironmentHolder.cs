namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface IK8sEnvironmentHolder
    {
        IK8sEnvironment? K8sEnvironment { get; internal set; }
    }
}
