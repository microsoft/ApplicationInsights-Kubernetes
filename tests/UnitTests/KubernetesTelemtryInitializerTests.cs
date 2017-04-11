using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    public class KubernetesTelemtryInitializerTests
    {
        [Fact(DisplayName = "K8sTelemetryInitializer gets null K8s environment when given null")]
        public void K8sTelemetryInitializerSetNullGetNull()
        {
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(null);
            Assert.Null(target.K8sEnvironment);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets the K8s env correct")]
        public void K8sTelemetryInitializerSetK8sEnvironment()
        {
            var envMock = new Mock<IK8sEnvironment>();
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(envMock.Object);

            Assert.NotNull(target.K8sEnvironment);
            Assert.Equal(envMock.Object, target.K8sEnvironment);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets the cloud_RoleName")]
        public void K8sTelemetryInitializerSetRoleName()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(envMock.Object);
            ITelemetry telemetry = new TraceTelemetry();
            target.Initialize(telemetry);

            Assert.Equal("Hello RoleName", telemetry.Context.Cloud.RoleName);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets custom dimensions")]
        public void K8sTelemetryInitializerSetsCustomDimensions()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");

            envMock.Setup(env => env.ContainerID).Returns("Cid");
            envMock.Setup(env => env.ContainerName).Returns("CName");
            envMock.Setup(env => env.PodID).Returns("Pid");
            envMock.Setup(env => env.PodName).Returns("PName");
            envMock.Setup(env => env.PodLabels).Returns("PLabels");
            envMock.Setup(env => env.ReplicaSetUid).Returns("Rid");
            envMock.Setup(env => env.DeploymentUid).Returns("Did");
            envMock.Setup(env => env.NodeUid).Returns("Nid");
            envMock.Setup(env => env.NodeName).Returns("NName");
            
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(envMock.Object);
            ITelemetry telemetry = new TraceTelemetry();
            target.Initialize(telemetry);

            Assert.Equal("Cid", telemetry.Context.Properties["K8s.Container.ID"]);
            Assert.Equal("CName", telemetry.Context.Properties["K8s.Container.Name"]);

            Assert.Equal("Pid", telemetry.Context.Properties["K8s.Pod.ID"]);
            Assert.Equal("PName", telemetry.Context.Properties["K8s.Pod.Name"]);
            Assert.Equal("PLabels", telemetry.Context.Properties["K8s.Pod.Labels"]);

            Assert.Equal("Rid", telemetry.Context.Properties["K8s.ReplicaSet.ID"]);

            Assert.Equal("Did", telemetry.Context.Properties["K8s.Deployment.ID"]);

            Assert.Equal("Nid", telemetry.Context.Properties["K8s.Node.ID"]);
            Assert.Equal("NName", telemetry.Context.Properties["K8s.Node.Name"]);
        }
    }
}
