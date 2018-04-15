using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using static Microsoft.ApplicationInsights.Netcore.Kubernetes.TestUtils;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    public class KuteHttpClientSettingsProviderTests
    {
        [Theory(DisplayName = "VerifyServerCertificate should verify the issuer")]
        [InlineData("SameIssuer", true)]
        [InlineData("DifferentIssuer", false)]
        public void VerifyServerCertificateShouldVerifyIssuer(string serverIssuer, bool expected)
        {

            Mock<ICertificateVerifier> serverCertMock = new Mock<ICertificateVerifier>();
            serverCertMock.Setup(cert => cert.Issuer).Returns(serverIssuer);
            serverCertMock.Setup(cert => cert.NotBefore).Returns(DateTime.Now.Date.AddDays(-1));
            serverCertMock.Setup(cert => cert.NotAfter).Returns(DateTime.Now.Date.AddDays(1));
            Mock<HttpRequestMessage> httpRequestMessageMock = new Mock<HttpRequestMessage>();
            Mock<X509Chain> chainMock = new Mock<X509Chain>();
            Mock<ICertificateVerifier> clientCertMock = new Mock<ICertificateVerifier>();
            clientCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");

            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true, GetLogger<KubeHttpClientSettingsProvider>());

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "VerifyServerCertificate should verify the cert valid period - valid date range")]
        public void VerifyServerCertificateShouldVerifyExpirationForValid()
        {
            Mock<ICertificateVerifier> serverCertMock = new Mock<ICertificateVerifier>();
            serverCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");
            serverCertMock.Setup(cert => cert.NotBefore).Returns(DateTime.Now.Date.AddDays(-1));
            serverCertMock.Setup(cert => cert.NotAfter).Returns(DateTime.Now.Date.AddDays(1));
            Mock<HttpRequestMessage> httpRequestMessageMock = new Mock<HttpRequestMessage>();
            Mock<X509Chain> chainMock = new Mock<X509Chain>();
            Mock<ICertificateVerifier> clientCertMock = new Mock<ICertificateVerifier>();
            clientCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");

            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true, GetLogger<KubeHttpClientSettingsProvider>());

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.True(actual);
        }

        [Fact(DisplayName = "VerifyServerCertificate should verify the cert valid period - edga case - 1 day certificate")]
        public void VerifyServerCertificateShouldVerifyExpirationSameDay()
        {
            Mock<ICertificateVerifier> serverCertMock = new Mock<ICertificateVerifier>();
            serverCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");
            serverCertMock.Setup(cert => cert.NotBefore).Returns(DateTime.Now.Date);
            serverCertMock.Setup(cert => cert.NotAfter).Returns(DateTime.Now.Date);
            Mock<HttpRequestMessage> httpRequestMessageMock = new Mock<HttpRequestMessage>();
            Mock<X509Chain> chainMock = new Mock<X509Chain>();
            Mock<ICertificateVerifier> clientCertMock = new Mock<ICertificateVerifier>();
            clientCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");

            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true, GetLogger<KubeHttpClientSettingsProvider>());

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.True(actual);
        }

        [Fact(DisplayName = "VerifyServerCertificate should verify the cert valid period - too early")]
        public void VerifyServerCertificateShouldVerifyExpirationEarly()
        {
            Mock<ICertificateVerifier> serverCertMock = new Mock<ICertificateVerifier>();
            serverCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");
            serverCertMock.Setup(cert => cert.NotBefore).Returns(DateTime.Now.Date.AddDays(1));
            serverCertMock.Setup(cert => cert.NotAfter).Returns(DateTime.Now.Date.AddDays(2));
            Mock<HttpRequestMessage> httpRequestMessageMock = new Mock<HttpRequestMessage>();
            Mock<X509Chain> chainMock = new Mock<X509Chain>();
            Mock<ICertificateVerifier> clientCertMock = new Mock<ICertificateVerifier>();
            clientCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");

            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true, GetLogger<KubeHttpClientSettingsProvider>());

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.False(actual);
        }

        [Fact(DisplayName = "VerifyServerCertificate should verify the cert valid period - too late")]
        public void VerifyServerCertificateShouldVerifyExpirationLate()
        {
            Mock<ICertificateVerifier> serverCertMock = new Mock<ICertificateVerifier>();
            serverCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");
            serverCertMock.Setup(cert => cert.NotBefore).Returns(DateTime.Now.Date.AddDays(-2));
            serverCertMock.Setup(cert => cert.NotAfter).Returns(DateTime.Now.Date.AddDays(-1));
            Mock<HttpRequestMessage> httpRequestMessageMock = new Mock<HttpRequestMessage>();
            Mock<X509Chain> chainMock = new Mock<X509Chain>();
            Mock<ICertificateVerifier> clientCertMock = new Mock<ICertificateVerifier>();
            clientCertMock.Setup(cert => cert.Issuer).Returns("SameIssuer");

            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true, GetLogger<KubeHttpClientSettingsProvider>());

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.False(actual);
        }

        [Fact(DisplayName = "ParseContainerId should return correct result")]
        public void ParseContainerIdShouldWork()
        {
            const string testCase_1 = "12:memory:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "11:freezer:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "10:devices:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "9:pids:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "8:hugetlb:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "7:net_cls,net_prio:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "6:cpu,cpuacct:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "5:perf_event:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "4:blkio:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "3:rdma:/\n" +
                "2:cpuset:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "1:name=systemd:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553";
            Assert.Equal("b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553", KubeHttpClientSettingsProvider.ParseContainerId(testCase_1));

            const string testCase_2 = "12:rdma:/\n" +
                "11:pids:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "10:memory:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "9:freezer:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "8:perf_event:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "7:blkio:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "6:hugetlb:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "5:cpuset:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "4:cpu,cpuacct:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "3:net_cls,net_prio:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "2:devices:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "1:name=systemd:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a";
            Assert.Equal("4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a", KubeHttpClientSettingsProvider.ParseContainerId(testCase_2));

            const string testCase_3 = "2:cpu,cpuacct:\n1:name=systemd:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a";
            Assert.Throws<InvalidCastException>(() => KubeHttpClientSettingsProvider.ParseContainerId(testCase_3));
        }

        [Fact(DisplayName = "Base address is formed by constructor")]
        public void BaseAddressShouldBeFormed()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(GetLogger<KubeHttpClientSettingsProvider>(),
                pathToCGroup: "TestCGroup",
                pathToNamespace: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.NotNull(target);
            Uri expected = new Uri("https://127.0.0.1:8001", UriKind.Absolute);
            Assert.Equal(expected.AbsoluteUri, target.ServiceBaseAddress.AbsoluteUri);
            Assert.Equal(expected.Port, target.ServiceBaseAddress.Port);
        }

        [Fact(DisplayName = "Base address is formed by constructor of windows kube settings provider")]
        public void BaseAddressShouldBeFormedWin()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpSettingsWinContainerProvider(GetLogger<KubeHttpSettingsWinContainerProvider>(),
                serviceAccountFolder: ".",
                namespaceFileName: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Uri expected = new Uri("https://127.0.0.1:8001", UriKind.Absolute);
            Assert.Equal(expected.AbsoluteUri, target.ServiceBaseAddress.AbsoluteUri);
            Assert.Equal(expected.Port, target.ServiceBaseAddress.Port);
        }

        [Fact(DisplayName = "Container id is set to null for windows container settings")]
        public void ContainerIdIsAlwaysNullForWinSettings()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpSettingsWinContainerProvider(GetLogger<KubeHttpSettingsWinContainerProvider>(),
                serviceAccountFolder: ".",
                namespaceFileName: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.Null(target.ContainerId);
        }

        [Fact(DisplayName = "Token can be fetched")]

        public void TokenShoudBeFetched()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(GetLogger<KubeHttpClientSettingsProvider>(),
                pathToCGroup: "TestCGroup",
                pathToNamespace: "namespace",
                pathToToken: "token",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.Equal("Test-token", target.GetToken());
        }

        [Fact(DisplayName = "Token can be fetched by windows settings provider")]
        public void TokenShouldBeFetchedForWin()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpSettingsWinContainerProvider(GetLogger<KubeHttpSettingsWinContainerProvider>(),
                serviceAccountFolder: ".",
                namespaceFileName: "namespace",
                tokenFileName:"token",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");

            Assert.Equal("Test-token", target.GetToken());
        }
    }
}
