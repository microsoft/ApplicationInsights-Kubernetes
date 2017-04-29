namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;

    internal interface ICertificateVerifier
    {
        string Issuer { get; }
        DateTime NotBefore { get; }
        DateTime NotAfter { get; }
    }
}
