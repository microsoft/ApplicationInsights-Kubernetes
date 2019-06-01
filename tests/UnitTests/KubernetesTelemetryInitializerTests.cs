using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class KubernetesTelemetryInitializerTests
    {
        [Fact(DisplayName = "K8sEnvFactory can't be null in K8sTelemetryInitializer")]
        public void ConstructorSetsNullGetsNull()
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() =>
            {
                KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                    null,
                    GetOptions(TimeSpan.FromSeconds(1)),
                    SDKVersionUtils.Instance);
            });

            Assert.Equal("Value cannot be null.\r\nParameter name: k8sEnvFactory", ex.Message);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets the K8s env correct")]
        public void ConstructorSetK8sEnvironment()
        {
            var envMock = new Mock<IK8sEnvironment>();
            var factoryMock = new Mock<IK8sEnvironmentFactory>();
            factoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                factoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance);

            Assert.NotNull(target._k8sEnvironment);
            Assert.Equal(factoryMock.Object, target._k8sEnvFactory);
            Assert.Equal(envMock.Object, target._k8sEnvironment);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets the cloud_RoleName")]
        public void InitializeSetsRoleName()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance);
            ITelemetry telemetry = new TraceTelemetry();
            target.Initialize(telemetry);

            Assert.Equal("Hello RoleName", telemetry.Context.Cloud.RoleName);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer will not overwrite the role name when it exists already.")]
        public void InitializeShouldNotOverwriteExistingRoleName()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("New RoleName");
            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance);
            ITelemetry telemetry = new TraceTelemetry();
            telemetry.Context.Cloud.RoleName = "Existing RoleName";
            target.Initialize(telemetry);

            Assert.Equal("Existing RoleName", telemetry.Context.Cloud.RoleName);
        }

        // [Fact]
        // public void InitializeWithEmptyForOptionalPropertyDoesNotLogError()
        // {
        //     var envMock = new Mock<IK8sEnvironment>();
        //     envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");

        //     envMock.Setup(env => env.ContainerID).Returns("Cid");
        //     envMock.Setup(env => env.ContainerName).Returns("CName");
        //     envMock.Setup(env => env.PodID).Returns("Pid");
        //     envMock.Setup(env => env.PodName).Returns("PName");
        //     envMock.Setup(env => env.PodLabels).Returns("PLabels");
        //     // The following properties are optional.
        //     envMock.Setup(env => env.ReplicaSetUid).Returns<string>(null);
        //     envMock.Setup(env => env.ReplicaSetName).Returns<string>(null);
        //     envMock.Setup(env => env.DeploymentUid).Returns<string>(null);
        //     envMock.Setup(env => env.DeploymentName).Returns<string>(null);
        //     envMock.Setup(env => env.NodeUid).Returns("Nid");
        //     envMock.Setup(env => env.NodeName).Returns("NName");

        //     var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
        //     envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

        //     var loggerMock = new Mock<ILogger<KubernetesTelemetryInitializer>>();

        //     KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
        //         envFactoryMock.Object,
        //         GetOptions(TimeSpan.FromSeconds(1)),
        //         SDKVersionUtils.Instance,
        //         loggerMock.Object);
        //     ITelemetry telemetry = new TraceTelemetry();
        //     target.Initialize(telemetry);
        //     loggerMock.Verify(l => l.Log(LogLevel.Error, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Never());
        // }

        // [Fact]
        // public void InitializeWithEmptyForRequiredPropertyDoesLogError()
        // {
        //     var envMock = new Mock<IK8sEnvironment>();
        //     envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");

        //     envMock.Setup(env => env.ContainerID).Returns("Cid");
        //     envMock.Setup(env => env.ContainerName).Returns("CName");
        //     envMock.Setup(env => env.PodID).Returns("Pid");
        //     envMock.Setup(env => env.PodName).Returns("PName");
        //     envMock.Setup(env => env.PodLabels).Returns("PLabels");
        //     envMock.Setup(env => env.ReplicaSetUid).Returns<string>(null);
        //     envMock.Setup(env => env.ReplicaSetName).Returns<string>(null);
        //     envMock.Setup(env => env.DeploymentUid).Returns<string>(null);
        //     envMock.Setup(env => env.DeploymentName).Returns<string>(null);
        //     // These 2 properties are required.
        //     envMock.Setup(env => env.NodeUid).Returns<string>(null);
        //     envMock.Setup(env => env.NodeName).Returns<string>(null);

        //     var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
        //     envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

        //     var loggerMock = new Mock<ILogger<KubernetesTelemetryInitializer>>();

        //     KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
        //         envFactoryMock.Object,
        //         GetOptions(TimeSpan.FromSeconds(1)),
        //         SDKVersionUtils.Instance,
        //         loggerMock.Object);
        //     ITelemetry telemetry = new TraceTelemetry();
        //     target.Initialize(telemetry);
        //     loggerMock.Verify(l => l.Log(LogLevel.Error, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Exactly(2));
        // }

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

            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance);
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
            Assert.NotNull(telemetry.Context.Properties["Process.CPU(%)"]);
            Assert.NotNull(telemetry.Context.Properties["Process.Memory"]);
#endif
        }

        [Fact(DisplayName = "K8sTelemetryInitializer will not overwrite custom dimension when it exists already.")]
        public void InitializeWillNotOverwriteExistingCustomDimension()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
            envMock.Setup(env => env.ContainerID).Returns("Cid");

            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance);
            ITelemetry telemetry = new TraceTelemetry();
            telemetry.Context.Properties["K8s.Container.ID"] = "Existing Cid";
            target.Initialize(telemetry);

            Assert.Equal("Existing Cid", telemetry.Context.Properties["K8s.Container.ID"]);
        }

        [Fact(DisplayName = "When timeout happens on fetching the Kubernetes properties, initializer fails gracefully")]
        public void TimeoutGettingK8sEnvNoException()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerID).Returns("Cid");
            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(envMock.Object, TimeSpan.FromMinutes(1));
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance);

            ITelemetry telemetry = new TraceTelemetry();
            telemetry.Context.Properties["K8s.Container.ID"] = "No Crash";
            target.Initialize(telemetry);

            Assert.Equal("No Crash", telemetry.Context.Properties["K8s.Container.ID"]);
        }

        [Fact(DisplayName = "Query Kubernetes Environment will timeout.")]
        public async Task QueryK8sEnvironmentWillTimeout()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerID).Returns("Cid");
            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(envMock.Object, TimeSpan.FromMinutes(1));
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance);
            Assert.False(target._isK8sQueryTimeout);
            await Task.Delay(TimeSpan.FromSeconds(1));
            Assert.True(target._isK8sQueryTimeout);
        }

        [Fact(DisplayName = "Slow K8s Env will not block the TelemetryInitializer")]
        public void SlowK8sEnvironmentBuildWillNotBlockTelemetryInitializerConstructor()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerID).Returns("Cid");
            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(envMock.Object, TimeSpan.FromMinutes(1));
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(30)),
                SDKVersionUtils.Instance);

            // K8s Environment is still null.
            Assert.Null(target._k8sEnvironment);
            // And is not yet timed out.
            Assert.False(target._isK8sQueryTimeout);
        }

        private IServiceProvider GetTestServiceProvider()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            return serviceCollection.BuildServiceProvider();
        }

        private IOptions<AppInsightsForKubernetesOptions> GetOptions(TimeSpan timeout)
        {
            return new OptionsWrapper<AppInsightsForKubernetesOptions>(new AppInsightsForKubernetesOptions()
            {
                InitializationTimeout = timeout,
            });
        }
    }
}
