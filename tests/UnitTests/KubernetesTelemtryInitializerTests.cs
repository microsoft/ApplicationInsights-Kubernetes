using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class KubernetesTelemtryInitializerTests
    {
        [Fact(DisplayName = "K8sTelemetryInitializer gets null K8s environment when given null")]
        public void ConstructorSetsNullGetsNull()
        {
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(null, null);
            Assert.Null(target.K8sEnvironment);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets the K8s env correct")]
        public void ConstructorSetK8sEnvironment()
        {
            var envMock = new Mock<IK8sEnvironment>();
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(null, envMock.Object);

            Assert.NotNull(target.K8sEnvironment);
            Assert.Equal(envMock.Object, target.K8sEnvironment);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets the cloud_RoleName")]
        public void InitializeSetsRoleName()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(null, envMock.Object);
            ITelemetry telemetry = new TraceTelemetry();
            target.Initialize(telemetry);

            Assert.Equal("Hello RoleName", telemetry.Context.Cloud.RoleName);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer will not overwrite the role name when it exists already.")]
        public void InitializeShouldNotOverwriteExistingRoleName()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("New RoleName");
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(null, envMock.Object);
            ITelemetry telemetry = new TraceTelemetry();
            telemetry.Context.Cloud.RoleName = "Existing RoleName";
            target.Initialize(telemetry);

            Assert.Equal("Existing RoleName", telemetry.Context.Cloud.RoleName);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets custom dimensions")]
        public void InitializeSetsCustomDimensions()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");

            envMock.Setup(env => env.ContainerID).Returns("Cid");
            envMock.Setup(env => env.ContainerName).Returns("CName");
            envMock.Setup(env => env.PodID).Returns("Pid");
            envMock.Setup(env => env.PodName).Returns("PName");
            envMock.Setup(env => env.PodLabels).Returns("PLabels");
            envMock.Setup(env => env.ReplicaSetUid).Returns("Rid");
            envMock.Setup(env => env.ReplicaSetName).Returns("RName");
            envMock.Setup(env => env.DeploymentUid).Returns("Did");
            envMock.Setup(env => env.DeploymentName).Returns("DName");
            envMock.Setup(env => env.NodeUid).Returns("Nid");
            envMock.Setup(env => env.NodeName).Returns("NName");

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(null, envMock.Object);
            ITelemetry telemetry = new TraceTelemetry();
            target.Initialize(telemetry);

            Assert.Equal("Cid", telemetry.Context.Properties["Kubernetes.Container.ID"]);
            Assert.Equal("CName", telemetry.Context.Properties["Kubernetes.Container.Name"]);

            Assert.Equal("Pid", telemetry.Context.Properties["Kubernetes.Pod.ID"]);
            Assert.Equal("PName", telemetry.Context.Properties["Kubernetes.Pod.Name"]);
            Assert.Equal("PLabels", telemetry.Context.Properties["Kubernetes.Pod.Labels"]);

            Assert.Equal("RName", telemetry.Context.Properties["Kubernetes.ReplicaSet.Name"]);

            Assert.Equal("DName", telemetry.Context.Properties["Kubernetes.Deployment.Name"]);

            Assert.Equal("Nid", telemetry.Context.Properties["Kubernetes.Node.ID"]);
            Assert.Equal("NName", telemetry.Context.Properties["Kubernetes.Node.Name"]);

#if !NETSTANDARD1_3 && !NETSTANDARD1_6
            Assert.NotNull(telemetry.Context.Properties["Process.CPU(ms)"]);
            Assert.NotNull(telemetry.Context.Properties["Process.Memory"]);
#endif
        }

        [Fact(DisplayName = "K8sTelemetryInitializer will not overwrite custom dimension when it exists already.")]
        public void InitializeWillNotOverwriteExistingCustomDimension()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");

            envMock.Setup(env => env.ContainerID).Returns("Cid");

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(null, envMock.Object);
            ITelemetry telemetry = new TraceTelemetry();
            telemetry.Context.Properties["K8s.Container.ID"] = "Existing Cid";
            target.Initialize(telemetry);

            Assert.Equal("Existing Cid", telemetry.Context.Properties["K8s.Container.ID"]);
        }
    }
}
