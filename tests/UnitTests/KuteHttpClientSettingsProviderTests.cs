using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Castle.Core.Logging;
using Microsoft.ApplicationInsights.Kubernetes;
using Moq;
using Xunit;

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
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true);

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
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true);

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.Equal(true, actual);
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
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true);

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.Equal(true, actual);
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
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true);

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.Equal(false, actual);
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
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(true);

            bool actual = target.VerifyServerCertificate(httpRequestMessageMock.Object,
                serverCertMock.Object,
                chainMock.Object,
                System.Net.Security.SslPolicyErrors.None,
                clientCertMock.Object);
            Assert.Equal(false, actual);
        }
    }
}
