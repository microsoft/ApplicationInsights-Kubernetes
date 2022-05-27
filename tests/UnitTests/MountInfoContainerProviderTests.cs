#nullable enable

using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;

public class MountInfoContainerIdProviderTests
{
    private const string expectedContainerId = "7a0144cee1256c539fab790199527b7051aff1b603ebcf7ed3fd436440ef3b3a";

    [Theory(DisplayName = "ParseContainerId should return correct result")]
    [InlineData($"678 655 254:1 /docker/containers/{expectedContainerId}/resolv.conf /etc/resolv.conf rw,relatime - ext4 /dev/vda1 rw")]
    [InlineData($"679 655 254:1 /docker/containers/{expectedContainerId}/hostname /etc/hostname rw,relatime - ext4 /dev/vda1 rw")]
    [InlineData($"680 655 254:1 /docker/containers/{expectedContainerId}/hosts /etc/hosts rw,relatime - ext4 /dev/vda1 rw")]
    public void ParseContainerIdShouldWork(string input)
    {

        MountInfoContainerIdProvider target = new MountInfoContainerIdProvider();
        bool success = target.TryParseContainerId(input, out string actual);

        Assert.True(success);
        Assert.Equal(expectedContainerId, actual);
    }
}
