using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;

public class TestK8sPodExtensionMethods
{
    [Fact]
    public void GetContainerStatusShallReturnTheStatusOnMatch()
    {
        string containerId = "b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4";
        string containerName = "testContainerName";
        K8sPod pod = new K8sPod();
        pod.Status = new K8sPodStatus()
        {
            ContainerStatuses = new ContainerStatus[]{
                new ContainerStatus(){
                    Name=containerName,
                    ContainerID = "docker://b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4"
                },
            },
        };

        ContainerStatus result = pod.GetContainerStatus(containerId);
        Assert.NotNull(result);
        Assert.Equal(containerName, result.Name);
    }

    [Fact]
    public void GetContainerStatusShallReturnNullWhenContainerIdNullOrEmpty()
    {
        K8sPod pod = new K8sPod();
        pod.Status = new K8sPodStatus()
        {
            ContainerStatuses = new ContainerStatus[]{
                new ContainerStatus(){
                    Name="testContainerName",
                    ContainerID = "docker://b1bf9cd89b57ba86c20e17bfd474638110e489da784a5e388983294d94ae9fc4"
                },
            },
        };

        ContainerStatus result = pod.GetContainerStatus(null);
        Assert.Null(result);

        ContainerStatus result2 = pod.GetContainerStatus(string.Empty);
        Assert.Null(result2);

        ContainerStatus result3 = pod.GetContainerStatus("");
        Assert.Null(result3);
    }
}
