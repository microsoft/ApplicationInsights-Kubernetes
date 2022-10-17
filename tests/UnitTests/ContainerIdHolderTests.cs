using System.Collections.Generic;
using k8s.Models;
using Microsoft.ApplicationInsights.Kubernetes.Containers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

[Collection(FullLoggingCollection.Name)]
public class ContainerIdHolderTests
{
    [Theory]
    [InlineData("cri-containerd-5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06.scope", "5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06")]
    public void ReturnedContainerIdIsNormalized(string input, string expect)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<IContainerIdProvider>(_ =>
        {
            Mock<IContainerIdProvider> containerIdProviderMock = new();
            // Container id returns pre-normalized container id.
            containerIdProviderMock.Setup(p => p.TryGetMyContainerId(out input)).Returns(true);
            return containerIdProviderMock.Object;
        });
        services.AddScoped<IContainerIdNormalizer, ContainerIdNormalizer>();


        ContainerIdHolder target = new ContainerIdHolder(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());

        // The container id returned is normalized.
        Assert.Equal(expect, target.ContainerId);
    }

    [Theory]
    [InlineData("cri-containerd-5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06.scope", "5146b2bcd77ab4f2624bc1fbd98cf9751741344a80b043dbd77a4e847bff4f06")]
    public void BackfilledContainerIdIsNormalized(string input, string expect)
    {
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<IContainerIdNormalizer, ContainerIdNormalizer>();

        V1Pod pod = new V1Pod()
        {
            Status = new V1PodStatus()
            {
                ContainerStatuses = new List<V1ContainerStatus>(){
                    new V1ContainerStatus(){ ContainerID = input},
                }
            }
        };

        ContainerIdHolder target = new ContainerIdHolder(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
        bool result = target.TryBackFillContainerId(pod, out V1ContainerStatus status);

        Assert.True(result);
        // The container id returned is normalized.
        Assert.Equal(expect, target.ContainerId);
    }

    [Fact]
    // A regression test to https://github.com/microsoft/ApplicationInsights-Kubernetes/issues/297.
    public void NoContainerIdShouldNotLeadToException()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<IContainerIdProvider>(_ =>
        {
            // No container id matched by the provider.
            Mock<IContainerIdProvider> containerIdProviderMock = new();
            string noContainerId = string.Empty;
            containerIdProviderMock.Setup(p => p.TryGetMyContainerId(out noContainerId)).Returns(false);
            return containerIdProviderMock.Object;
        });
        services.AddScoped<IContainerIdNormalizer, ContainerIdNormalizer>();

        ContainerIdHolder target = new ContainerIdHolder(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());

        // Validate that no exception is thrown.
        Assert.NotNull(target);
        Assert.Equal(string.Empty, target.ContainerId);
    }
}
