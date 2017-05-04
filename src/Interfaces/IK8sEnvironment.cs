namespace Microsoft.ApplicationInsights.Kubernetes
{
    public interface IK8sEnvironment
    {
        string ContainerID { get; }
        string ContainerName { get; }
        string DeploymentUid { get; }
        string DeploymentName { get; }
        string NodeName { get; }
        string NodeUid { get; }
        string PodID { get; }
        string PodLabels { get; }
        string PodName { get; }
        string ReplicaSetUid { get; }
        string ReplicaSetName { get; }
    }
}