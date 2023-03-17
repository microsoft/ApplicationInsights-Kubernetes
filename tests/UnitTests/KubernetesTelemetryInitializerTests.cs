using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes.Tests;

[Collection(FullLoggingCollection.Name)]
public class KubernetesTelemetryInitializerTests
{
    [Fact(DisplayName = "K8sEnvironmentHolder can't be null in K8sTelemetryInitializer")]
    public void ConstructorSetsNullGetsNull()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
        {
            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                null,
                SDKVersionUtils.Instance,
                keyCacheMock.Object
                );
        });

        Assert.Equal("k8sEnvironmentHolder", ex.ParamName);
    }

    [Fact(DisplayName = "K8sTelemetryInitializer sets the K8s env correct")]
    public void ConstructorSetK8sEnvironment()
    {
        Mock<IK8sEnvironment> k8sEnvMock = new();
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        k8sEnvironmentHolderMock.Setup(h => h.K8sEnvironment).Returns(k8sEnvMock.Object);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
            k8sEnvironmentHolderMock.Object,
            SDKVersionUtils.Instance,
            keyCacheMock.Object);

        Assert.Equal(k8sEnvironmentHolderMock.Object, target.K8SEnvironmentHolder);
        Assert.Equal(keyCacheMock.Object, target.TelemetryKeyCache);
    }

    [Fact(DisplayName = "K8sTelemetryInitializer sets the cloud_RoleName")]
    public void InitializeSetsRoleName()
    {
        Mock<IK8sEnvironment> envMock = new();
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
        k8sEnvironmentHolderMock.Setup(h => h.K8sEnvironment).Returns(envMock.Object);
        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(k8sEnvironmentHolderMock.Object,
            SDKVersionUtils.Instance,
            keyCacheMock.Object);
        ITelemetry telemetry = new TraceTelemetry();
        target.Initialize(telemetry);

        Assert.Equal("Hello RoleName", telemetry.Context.Cloud.RoleName);
    }

    [Fact(DisplayName = "K8sTelemetryInitializer will not overwrite the role name when it exists already.")]
    public void InitializeShouldNotOverwriteExistingRoleName()
    {
        Mock<IK8sEnvironment> envMock = new();
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
        k8sEnvironmentHolderMock.Setup(h => h.K8sEnvironment).Returns(envMock.Object);
        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
            k8sEnvironmentHolderMock.Object,
            SDKVersionUtils.Instance,
            keyCacheMock.Object);

        ITelemetry telemetry = new TraceTelemetry();
        telemetry.Context.Cloud.RoleName = "Existing RoleName";
        target.Initialize(telemetry);

        Assert.Equal("Existing RoleName", telemetry.Context.Cloud.RoleName);
    }

    [Fact]
    public void InitializeWithEmptyForOptionalPropertyDoesNotLogError()
    {
        var listener = new TestDiagnosticSourceObserver();
        ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(listener);

        // Mocks needed        
        Mock<IK8sEnvironment> envMock = new();
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        // Setup
        envMock.Setup(env => env.ContainerID).Returns("Cid");
        envMock.Setup(env => env.ContainerName).Returns("CName");
        envMock.Setup(env => env.ImageName).Returns("IName");
        envMock.Setup(env => env.PodID).Returns("Pid");
        envMock.Setup(env => env.PodName).Returns("PName");
        envMock.Setup(env => env.PodLabels).Returns("PLabels");
        // The following properties are optional.
        envMock.Setup(env => env.ReplicaSetUid).Returns<string>(null);
        envMock.Setup(env => env.ReplicaSetName).Returns<string>(null);
        envMock.Setup(env => env.DeploymentUid).Returns<string>(null);
        envMock.Setup(env => env.DeploymentName).Returns<string>(null);
        envMock.Setup(env => env.PodNamespace).Returns<string>(null);
        envMock.Setup(env => env.NodeUid).Returns<string>(null);
        envMock.Setup(env => env.NodeName).Returns<string>(null);

        k8sEnvironmentHolderMock.Setup(h => h.K8sEnvironment).Returns(envMock.Object);
        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
            k8sEnvironmentHolderMock.Object,
            SDKVersionUtils.Instance,
            keyCacheMock.Object);
        ITelemetry telemetry = new TraceTelemetry();
        target.Initialize(telemetry);

        Assert.Equal(0, listener.GetCount(DiagnosticLogLevel.Error));
    }

    [Fact]
    public void InitializeWithEmptyForRequiredPropertyDoesLogError()
    {
        var listener = new TestDiagnosticSourceObserver();
        ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(listener);

        // Mocks needed        
        Mock<IK8sEnvironment> envMock = new();
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        // Setup
        envMock.Setup(env => env.ContainerID).Returns("Cid");
        envMock.Setup(env => env.ContainerName).Returns("CName");
        envMock.Setup(env => env.ImageName).Returns("IName");
        envMock.Setup(env => env.PodLabels).Returns("PLabels");
        envMock.Setup(env => env.ReplicaSetUid).Returns<string>(null);
        envMock.Setup(env => env.ReplicaSetName).Returns<string>(null);
        envMock.Setup(env => env.DeploymentUid).Returns<string>(null);
        envMock.Setup(env => env.DeploymentName).Returns<string>(null);
        envMock.Setup(env => env.PodNamespace).Returns<string>(null);
        envMock.Setup(env => env.NodeUid).Returns<string>(null);
        envMock.Setup(env => env.NodeName).Returns<string>(null);
        // These 2 properties are required.
        envMock.Setup(env => env.PodID).Returns<string>(null);
        envMock.Setup(env => env.PodName).Returns<string>(null);

        k8sEnvironmentHolderMock.Setup(h => h.K8sEnvironment).Returns(envMock.Object);
        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
            k8sEnvironmentHolderMock.Object,
            SDKVersionUtils.Instance,
            keyCacheMock.Object);
        ITelemetry telemetry = new TraceTelemetry();
        target.Initialize(telemetry);

        Assert.Equal(2, listener.GetCount(DiagnosticLogLevel.Error));
    }

    [Fact(DisplayName = "K8sTelemetryInitializer sets custom dimensions")]
    public void InitializeSetsCustomDimensions()
    {
        Mock<IK8sEnvironment> envMock = new();
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        // Setup
        envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
        envMock.Setup(env => env.ContainerID).Returns("Cid");
        envMock.Setup(env => env.ContainerName).Returns("CName");
        envMock.Setup(env => env.ImageName).Returns("IName");
        envMock.Setup(env => env.PodID).Returns("Pid");
        envMock.Setup(env => env.PodName).Returns("PName");
        envMock.Setup(env => env.PodLabels).Returns("PLabels");
        envMock.Setup(env => env.ReplicaSetUid).Returns("Rid");
        envMock.Setup(env => env.ReplicaSetName).Returns("RName");
        envMock.Setup(env => env.DeploymentUid).Returns("Did");
        envMock.Setup(env => env.DeploymentName).Returns("DName");
        envMock.Setup(env => env.NodeUid).Returns("Nid");
        envMock.Setup(env => env.NodeName).Returns("NName");
        envMock.Setup(env => env.PodNamespace).Returns("PNS");

        k8sEnvironmentHolderMock.Setup(h => h.K8sEnvironment).Returns(envMock.Object);
        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
            k8sEnvironmentHolderMock.Object,
            SDKVersionUtils.Instance,
            keyCacheMock.Object);
        ITelemetry telemetry = new TraceTelemetry();
        target.Initialize(telemetry);

        ISupportProperties telemetryWithProperties = telemetry as ISupportProperties;

        Assert.Equal("Cid", telemetryWithProperties.Properties["Kubernetes.Container.ID"]);
        Assert.Equal("CName", telemetryWithProperties.Properties["Kubernetes.Container.Name"]);

        Assert.Equal("Pid", telemetryWithProperties.Properties["Kubernetes.Pod.ID"]);
        Assert.Equal("PName", telemetryWithProperties.Properties["Kubernetes.Pod.Name"]);
        Assert.Equal("PLabels", telemetryWithProperties.Properties["Kubernetes.Pod.Labels"]);
        Assert.Equal("PNS", telemetryWithProperties.Properties["Kubernetes.Pod.Namespace"]);

        Assert.Equal("RName", telemetryWithProperties.Properties["Kubernetes.ReplicaSet.Name"]);

        Assert.Equal("DName", telemetryWithProperties.Properties["Kubernetes.Deployment.Name"]);

        Assert.Equal("Nid", telemetryWithProperties.Properties["Kubernetes.Node.ID"]);
        Assert.Equal("NName", telemetryWithProperties.Properties["Kubernetes.Node.Name"]);
    }

    [Fact(DisplayName = "K8sTelemetryInitializer will not overwrite custom dimension when it exists already.")]
    public void InitializeWillNotOverwriteExistingCustomDimension()
    {
        Mock<IK8sEnvironment> envMock = new();
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
        envMock.Setup(env => env.ContainerID).Returns("Cid");

        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);
        k8sEnvironmentHolderMock.Setup(h => h.K8sEnvironment).Returns(envMock.Object);
        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
            k8sEnvironmentHolderMock.Object,
            SDKVersionUtils.Instance,
            keyCacheMock.Object);
        ITelemetry telemetry = new TraceTelemetry();
        ISupportProperties telemetryWithProperties = telemetry as ISupportProperties;
        telemetryWithProperties.Properties["K8s.Container.ID"] = "Existing Cid";
        target.Initialize(telemetry);

        Assert.Equal("Existing Cid", telemetryWithProperties.Properties["K8s.Container.ID"]);
    }

    [Fact(DisplayName = "When timeout happens on fetching the Kubernetes properties, initializer fails gracefully")]
    public void TimeoutGettingK8sEnvNoException()
    {
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
            k8sEnvironmentHolderMock.Object,    // k8sEnvironmentHolderMock.Object.K8sEnvironment is null
            SDKVersionUtils.Instance,
            keyCacheMock.Object);

        ITelemetry telemetry = new TraceTelemetry();
        ISupportProperties telemetryWithProperties = telemetry as ISupportProperties;

        telemetryWithProperties.Properties["K8s.Container.ID"] = "No Crash";
        target.Initialize(telemetry);

        Assert.Equal("No Crash", telemetryWithProperties.Properties["K8s.Container.ID"]);
    }

    [Fact(DisplayName = "K8sTelemetryInitializer make use of the key by the processor provided in the options.")]
    public void ShouldUseTheValueByTheKeyProcessorForTelemetry()
    {
        Mock<IK8sEnvironment> envMock = new();
        Mock<IK8sEnvironmentHolder> k8sEnvironmentHolderMock = new();
        Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();

        envMock.Setup(env => env.ContainerName).Returns("Hello.RoleName");
        envMock.Setup(env => env.ContainerID).Returns("Hello.Cid");

        keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input.Replace('.', '_'));
        k8sEnvironmentHolderMock.Setup(h => h.K8sEnvironment).Returns(envMock.Object);

        KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
            k8sEnvironmentHolderMock.Object,
            SDKVersionUtils.Instance,
            keyCacheMock.Object);
        ITelemetry telemetry = new TraceTelemetry();
        target.Initialize(telemetry);

        ISupportProperties telemetryWithProperties = telemetry as ISupportProperties;

        Assert.False(telemetryWithProperties.Properties.ContainsKey("Kubernetes.Container.ID"));
        Assert.True(telemetryWithProperties.Properties.ContainsKey("Kubernetes_Container_ID"));
    }
}
