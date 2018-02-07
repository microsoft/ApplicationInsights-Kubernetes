namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    internal interface ICertificateVerifier
    {
        X509Certificate2 Certificate { get; }
        string Issuer { get; }
        DateTime NotBefore { get; }
        DateTime NotAfter { get; }
    }
}
