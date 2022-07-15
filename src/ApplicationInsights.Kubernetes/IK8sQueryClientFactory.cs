namespace Microsoft.ApplicationInsights.Kubernetes;

internal interface IK8sQueryClientFactory
{
    IK8sQueryClient Create();
}
