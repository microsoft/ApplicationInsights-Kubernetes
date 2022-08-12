using System;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders.Tests;

public class ContainerIdNormalizerTests
{
    [Theory]
    // With prefix and suffix:
    [InlineData(@"cri-containerd-5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06.scope", "5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06")]
    // With prefix only:
    [InlineData(@"docker://5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06", "5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06")]
    // With suffix only:
    [InlineData(@"5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06-scope", "5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06")]
    // Same as normalized:
    [InlineData(@"5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06", "5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06")]
    // Longer than 64 digits - notes: so that the match regex is simplified.
    [InlineData(@"5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06a", "5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06")]
    public void TryGetNormalizedShouldNormalizeContainerIds(string input, string expected)
    {
        ContainerIdNormalizer target = new ContainerIdNormalizer();
        bool result = target.TryNormalize(input, out string actual);

        Assert.True(result);
        Assert.Equal(expected, actual);
    }

    [Theory]
    // Input has no container id
    [InlineData("Input has no container id")]
    // Short guid is not accepted
    [InlineData("f78375b1c487")]
    // Shorter than 64 digits
    [InlineData("5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f0")]
    // Not a valid guid with character of z
    [InlineData("5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f0z")]
    public void TryGetNormalizedShouldNotAcceptInvalidContainerIds(string input)
    {
        ContainerIdNormalizer target = new ContainerIdNormalizer();
        bool result = target.TryNormalize(input, out string actual);

        Assert.False(result);
        Assert.Null(actual);
    }

    [Fact]
    public void TryGetNormalizedShouldHandleStringEmpty()
    {
        ContainerIdNormalizer target = new ContainerIdNormalizer();
        bool result = target.TryNormalize(string.Empty, out string actual);

        Assert.True(result);
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void TryGetNormalizedShouldDoesNotAcceptNull()
    {
        ContainerIdNormalizer target = new ContainerIdNormalizer();
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = target.TryNormalize(null, out string actual);
        });
    }
}
