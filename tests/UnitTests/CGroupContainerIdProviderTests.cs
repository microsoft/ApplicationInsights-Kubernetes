using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

[Collection(FullLoggingCollection.Name)]
public class CGroupContainerIdProviderTests
{
    [Theory]
    [InlineData(@"2:cpuset:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553", "b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553")]
    [InlineData(@"5:cpuset:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a", "4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a")]
    public void ParseCGroupV1ShouldWork(string input, string expected)
    {
        CGroupV1Matcher target = new CGroupV1Matcher();
        bool result = target.TryParseContainerId(input, out string actual);

        Assert.True(result);
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(@"1:name=systemd:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a")]
    [InlineData(@"12:memory:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553")]
    [InlineData(@"7:blkio:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a")]
    public void ParseCGroupV1ShouldReturnNull(string input)
    {
        CGroupV1Matcher target = new CGroupV1Matcher();
        bool result = target.TryParseContainerId(input, out string actual);

        Assert.False(result);
        Assert.Empty(actual);
    }
}
