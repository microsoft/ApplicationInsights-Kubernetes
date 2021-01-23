using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using Microsoft.ApplicationInsights.Kubernetes.Utilities;
using Microsoft.Extensions.DependencyInjection;
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
                Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
                KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                    null,
                    GetOptions(TimeSpan.FromSeconds(1)),
                    SDKVersionUtils.Instance,
                    keyCacheMock.Object
                    );
            });

            Assert.Equal("Value cannot be null.\r\nParameter name: k8sEnvFactory", ex.Message);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets the K8s env correct")]
        public void ConstructorSetK8sEnvironment()
        {
            var envMock = new Mock<IK8sEnvironment>();
            var factoryMock = new Mock<IK8sEnvironmentFactory>();
            factoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                factoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance,
                keyCacheMock.Object);

            Assert.NotNull(target._k8sEnvironment);
            Assert.Equal(factoryMock.Object, target._k8sEnvFactory);
            Assert.Equal(envMock.Object, target._k8sEnvironment);
            Assert.Equal(keyCacheMock.Object, target._telemetryKeyCache);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer sets the cloud_RoleName")]
        public void InitializeSetsRoleName()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);
            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance,
                keyCacheMock.Object);
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
            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
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

            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");

            envMock.Setup(env => env.ContainerID).Returns("Cid");
            envMock.Setup(env => env.ContainerName).Returns("CName");
            envMock.Setup(env => env.PodID).Returns("Pid");
            envMock.Setup(env => env.PodName).Returns("PName");
            envMock.Setup(env => env.PodLabels).Returns("PLabels");

            // The following properties are optional.
            envMock.Setup(env => env.ReplicaSetUid).Returns<string>(null);
            envMock.Setup(env => env.ReplicaSetName).Returns<string>(null);
            envMock.Setup(env => env.DeploymentUid).Returns<string>(null);
            envMock.Setup(env => env.DeploymentName).Returns<string>(null);
            envMock.Setup(env => env.PodNamespace).Returns<string>(null);
            envMock.Setup(env => env.NodeUid).Returns("Nid");
            envMock.Setup(env => env.NodeName).Returns("NName");

            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
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

            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");

            envMock.Setup(env => env.ContainerID).Returns("Cid");
            envMock.Setup(env => env.ContainerName).Returns("CName");
            envMock.Setup(env => env.PodID).Returns("Pid");
            envMock.Setup(env => env.PodName).Returns("PName");
            envMock.Setup(env => env.PodLabels).Returns("PLabels");
            envMock.Setup(env => env.ReplicaSetUid).Returns<string>(null);
            envMock.Setup(env => env.ReplicaSetName).Returns<string>(null);
            envMock.Setup(env => env.DeploymentUid).Returns<string>(null);
            envMock.Setup(env => env.DeploymentName).Returns<string>(null);
            envMock.Setup(env => env.PodNamespace).Returns<string>(null);
            // These 2 properties are required.
            envMock.Setup(env => env.NodeUid).Returns<string>(null);
            envMock.Setup(env => env.NodeName).Returns<string>(null);

            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance,
                keyCacheMock.Object);
            ITelemetry telemetry = new TraceTelemetry();
            target.Initialize(telemetry);

            Assert.Equal(2, listener.GetCount(DiagnosticLogLevel.Error));
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
            envMock.Setup(env => env.PodNamespace).Returns("PNS");


            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
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
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello RoleName");
            envMock.Setup(env => env.ContainerID).Returns("Cid");

            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
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
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerID).Returns("Cid");
            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(envMock.Object, TimeSpan.FromMinutes(1));
            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance,
                keyCacheMock.Object);

            ITelemetry telemetry = new TraceTelemetry();
            ISupportProperties telemetryWithProperties = telemetry as ISupportProperties;

            telemetryWithProperties.Properties["K8s.Container.ID"] = "No Crash";
            target.Initialize(telemetry);

            Assert.Equal("No Crash", telemetryWithProperties.Properties["K8s.Container.ID"]);
        }

        [Fact(DisplayName = "Query Kubernetes Environment will timeout.")]
        public async Task QueryK8sEnvironmentWillTimeout()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerID).Returns("Cid");
            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(envMock.Object, TimeSpan.FromMinutes(1));
            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance,
                keyCacheMock.Object);
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
            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input);

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(30)),
                SDKVersionUtils.Instance,
                keyCacheMock.Object);

            // K8s Environment is still null.
            Assert.Null(target._k8sEnvironment);
            // And is not yet timed out.
            Assert.False(target._isK8sQueryTimeout);
        }

        [Fact(DisplayName = "K8sTelemetryInitializer make use of the key by the processor provided in the options.")]
        public void ShouldUseTheValueByTheKeyProcessorForTelemetry()
        {
            var envMock = new Mock<IK8sEnvironment>();
            envMock.Setup(env => env.ContainerName).Returns("Hello.RoleName");
            envMock.Setup(env => env.ContainerID).Returns("Hello.Cid");

            var envFactoryMock = new Mock<IK8sEnvironmentFactory>();
            envFactoryMock.Setup(f => f.CreateAsync(It.IsAny<DateTime>())).ReturnsAsync(() => envMock.Object);

            Mock<ITelemetryKeyCache> keyCacheMock = new Mock<ITelemetryKeyCache>();
            keyCacheMock.Setup(c => c.GetProcessedKey(It.IsAny<string>())).Returns<string>(input => input.Replace('.', '_'));

            KubernetesTelemetryInitializer target = new KubernetesTelemetryInitializer(
                envFactoryMock.Object,
                GetOptions(TimeSpan.FromSeconds(1)),
                SDKVersionUtils.Instance,
                keyCacheMock.Object);
            ITelemetry telemetry = new TraceTelemetry();
            target.Initialize(telemetry);

            ISupportProperties telemetryWithProperties = telemetry as ISupportProperties;
            
            Assert.False(telemetryWithProperties.Properties.ContainsKey("Kubernetes.Container.ID"));
            Assert.True(telemetryWithProperties.Properties.ContainsKey("Kubernetes_Container_ID"));
        }

        private IServiceProvider GetTestServiceProvider()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
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
