namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    internal class CertificateVerifier : ICertificateVerifier
    {
        X509Certificate2 certificate;
        public CertificateVerifier(X509Certificate2 certificate)
        {
            this.certificate = Arguments.IsNotNull(certificate, nameof(certificate));
        }

        public X509Certificate2 Certificate => this.certificate;

        public string Issuer => this.certificate.Issuer;

        public DateTime NotBefore => this.certificate.NotBefore;

        public DateTime NotAfter => this.certificate.NotAfter;
    }
}
