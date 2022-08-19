using System;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

[Collection(FullLoggingCollection.Name)]
public class EnvironmentVariablePodNameProviderTests
{
    [Fact(DisplayName = $"{nameof(EnvironmentVariablePodNameProviderBase.TryGetPodName)} should get value back.")]
    public void TryGetPodNameShouldReturnValue()
    {
        Environment.SetEnvironmentVariable(HostNamePodNameProvider.VariableName, "def");
        // Simply testing the capability with the derived class
        EnvironmentVariablePodNameProviderBase target = new HostNamePodNameProvider();
        bool result = target.TryGetPodName(out string actual);

        Assert.True(result);
        Assert.Equal("def", actual);
    }
}
