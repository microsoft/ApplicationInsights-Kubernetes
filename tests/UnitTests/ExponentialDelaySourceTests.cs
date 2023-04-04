using System;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

public class ExponentialDelaySourceTests
{
    [Fact]
    public void ShouldReturnTheProperTimeSpan()
    {
        ExponentialDelaySource target = new(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5));

        TimeSpan firstTime = target.GetNext();
        Assert.Equal(TimeSpan.FromSeconds(5), firstTime);

        TimeSpan secondTime = target.GetNext();
        Assert.Equal(TimeSpan.FromSeconds(25), secondTime);

        TimeSpan third = target.GetNext();
        Assert.Equal(TimeSpan.FromSeconds(125), third);
    }

    [Fact]
    public void ShouldHitTheMaximum()
    {
        ExponentialDelaySource target = new(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));

        TimeSpan firstTime = target.GetNext();
        Assert.Equal(TimeSpan.FromSeconds(5), firstTime);

        TimeSpan secondTime = target.GetNext();
        Assert.Equal(TimeSpan.FromSeconds(25), secondTime);

        TimeSpan third = target.GetNext();
        Assert.Equal(TimeSpan.FromSeconds(60), third);
    }
}
