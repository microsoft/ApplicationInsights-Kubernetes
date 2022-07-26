using k8s.Models;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;

public class TestK8sPodExtensionMethods
{
    [Fact]
    public void GetContainerStatusShallReturnTheStatusOnMatch()
    {
        string containerId = "b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4";
        string containerName = "testContainerName";
        V1Pod pod = new V1Pod();
        pod.Status = new V1PodStatus()
        {
            ContainerStatuses = new V1ContainerStatus[]{
                new V1ContainerStatus(){
                    Name=containerName,
                    ContainerID = "docker://b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4"
                },
            },
        };

        V1ContainerStatus result = pod.GetContainerStatus(containerId);
        Assert.NotNull(result);
        Assert.Equal(containerName, result.Name);
    }

    [Fact]
    public void GetContainerStatusShallReturnNullWhenContainerIdNullOrEmpty()
    {
        V1Pod pod = new V1Pod();
        pod.Status = new V1PodStatus()
        {
            ContainerStatuses = new V1ContainerStatus[]{
                new V1ContainerStatus(){
                    Name="testContainerName",
                    ContainerID = "docker://b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4"
                },
            },
        };

        V1ContainerStatus result = pod.GetContainerStatus(null);
        Assert.Null(result);

        V1ContainerStatus result2 = pod.GetContainerStatus(string.Empty);
        Assert.Null(result2);

        V1ContainerStatus result3 = pod.GetContainerStatus("");
        Assert.Null(result3);
    }
}
