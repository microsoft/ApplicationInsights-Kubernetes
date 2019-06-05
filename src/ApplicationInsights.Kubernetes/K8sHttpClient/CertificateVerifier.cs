namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    internal class CertificateVerifier : ICertificateVerifier
    {
        readonly X509Certificate2 _certificate;
        public CertificateVerifier(X509Certificate2 certificate)
        {
            _certificate = Arguments.IsNotNull(certificate, nameof(certificate));
        }

        public X509Certificate2 Certificate => _certificate;

        public string Issuer => _certificate.Issuer;

        public DateTime NotBefore => _certificate.NotBefore;

        public DateTime NotAfter => _certificate.NotAfter;
    }
}
