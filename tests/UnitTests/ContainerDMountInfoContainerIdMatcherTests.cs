#nullable enable

using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;

public class ContainerDMountInfoContainerIdMatcherTests
{
    private const string expectedContainerId = "7a0144cee1256c539fab790199527b7051aff1b603ebcf7ed3fd436440ef3b3a";

    [Theory(DisplayName = "ParseContainerId should return correct result")]
    [InlineData($"1733 1729 0:35 /kubepods/besteffort/pod3272f253-be44-4a82-a541-9083e68cf99f/{expectedContainerId} /sys/fs/cgroup/blkio ro,nosuid,nodev,noexec,relatime master:17 - cgroup cgroup rw,blkio")]
    [InlineData($"1736 1729 0:38 /kubepods/besteffort/pod3272f253-be44-4a82-a541-9083e68cf99f/{expectedContainerId} /sys/fs/cgroup/freezer ro,nosuid,nodev,noexec,relatime master:20 - cgroup cgroup rw,freezer")]
    [InlineData($"1736 1729 0:38 /kubepods/besteffort/pod3272f253-be44-4a82-a541-9083e68cf99f/{expectedContainerId}/ /sys/fs/cgroup/freezer ro,nosuid,nodev,noexec,relatime master:20 - cgroup cgroup rw,freezer")] // A little bit wiggling room to having tailing slash
    public void ParseContainerIdShouldWork(string input)
    {
        IContainerIdMatcher target = new ContainerDMountInfoMatcher();
        bool result = target.TryParseContainerId(input, out string actual);

        Assert.True(result);
        Assert.NotNull(actual);
        Assert.Equal(expectedContainerId, actual);
    }

    [Theory(DisplayName = "ParseContainerId should return correct result")]
    [InlineData($"1733 1729 0:35 /not/expected/path/{expectedContainerId} /sys/fs/cgroup/blkio ro,nosuid,nodev,noexec,relatime master:17 - cgroup cgroup rw,blkio")]
    public void ParseContainerIdShouldReturnEmptyOnNoMatch(string input)
    {
        IContainerIdMatcher target = new ContainerDMountInfoMatcher();
        bool result = target.TryParseContainerId(input, out string actual);

        Assert.False(result);
        Assert.Empty(actual);
    }
}
