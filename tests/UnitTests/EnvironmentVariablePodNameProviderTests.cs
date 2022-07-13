using System;
using Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes;

public class EnvironmentVariablePodNameProviderTests
{
    [Fact(DisplayName = $"{nameof(EnvironmentVariablePodNameProvider.TryGetPodName)} should get value back.")]
    public void TryGetPodNameShouldReturnValue()
    {
        Environment.SetEnvironmentVariable("abc", "def");
        EnvironmentVariablePodNameProvider target = new EnvironmentVariablePodNameProvider("abc");
        bool result = target.TryGetPodName(out string actual);

        Assert.True(result);
        Assert.Equal("def", actual);
    }
}
