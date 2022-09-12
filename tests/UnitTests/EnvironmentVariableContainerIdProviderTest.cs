using System;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

[Collection(FullLoggingCollection.Name)]
public class EnvironmentVariableContainerIdProviderTests
{
    [Fact]
    public void ShouldBeAbleToFetchEnvironmentVariableWhenSet()
    {
        string expected = Guid.NewGuid().ToString("n");
        Environment.SetEnvironmentVariable("ContainerId", expected);

        EnvironmentVariableContainerIdProvider target = new EnvironmentVariableContainerIdProvider();
        bool result =target.TryGetMyContainerId(out string actual);

        Assert.True(result);
        Assert.Equal(expected, actual);
    }
}
