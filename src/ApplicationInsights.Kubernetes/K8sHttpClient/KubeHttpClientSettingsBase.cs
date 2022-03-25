#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal abstract class KubeHttpClientSettingsBase : IKubeHttpClientSettingsProvider
    {
        private readonly IEnumerable<IContainerIdProvider> _containerIdProviders;
        protected readonly ApplicationInsightsKubernetesDiagnosticSource _logger = ApplicationInsightsKubernetesDiagnosticSource.Instance;

        public KubeHttpClientSettingsBase(
            string? kubernetesServiceHost,
            string? kubernetesServicePort,
            IEnumerable<IContainerIdProvider> containerIdProviders)
        {
            kubernetesServiceHost = kubernetesServiceHost ?? Environment.GetEnvironmentVariable(@"KUBERNETES_SERVICE_HOST");
            if (string.IsNullOrEmpty(kubernetesServiceHost))
            {
                throw new NullReferenceException("Kubernetes service host is not set.");
            }

            kubernetesServicePort = kubernetesServicePort ?? Environment.GetEnvironmentVariable(@"KUBERNETES_SERVICE_PORT");
            if (string.IsNullOrEmpty(kubernetesServicePort))
            {
                throw new NullReferenceException("Kubernetes service port is not set.");
            }

            string baseAddress = Invariant($"https://{kubernetesServiceHost}:{kubernetesServicePort}/");
            _logger.LogDebug("Kubernetes base address: {0}", baseAddress);
            ServiceBaseAddress = new Uri(baseAddress, UriKind.Absolute);
            _containerIdProviders = containerIdProviders ?? throw new ArgumentNullException(nameof(containerIdProviders));

            ContainerId = GetContainerIdOrThrow();
        }

        /// <summary>
        /// Gets the container Id by best effort.
        /// </summary>
        /// <remarks>
        /// Notice, containerId can't be null but could be string.Empty in environment like Windows Container.
        /// </remarks>
        public string ContainerId { get; }

        public abstract string QueryNamespace { get; }

        public Uri ServiceBaseAddress { get; private set; }

        public virtual HttpMessageHandler CreateMessageHandler()
        {
            string certFilePath = GetCertFilePath();
            if (!File.Exists(certFilePath))
            {
                throw new FileNotFoundException("Certificate file is required to access kubernetes API.", certFilePath);
            }

            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, serverCert, chain, policyErrors) =>
                {
                    X509Certificate2 clientCert = new X509Certificate2(certFilePath);
                    return CertificateValidationCallBack(httpRequestMessage, serverCert, clientCert, chain, policyErrors);
                }
            };

            return handler;
        }

        /// <summary>
        /// SSl Cert Validation Callback
        /// </summary>
        /// <param name="requestMessage">Http request message.</param>
        /// <param name="caCert">Server certificate.</param>
        /// <param name="clientCertificate">Client certificate</param>
        /// <param name="chain">Chain</param>
        /// <param name="sslPolicyErrors">SSL policy errors</param>
        /// <returns>true if valid cert</returns>
        internal bool CertificateValidationCallBack(
#pragma warning disable CA1801 // Unused by design
            HttpRequestMessage requestMessage,
#pragma warning restore CA1801 // Restore the warning
            X509Certificate2 caCert,
            X509Certificate2 clientCertificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                _logger.LogTrace("This is a valid signed certificate.");
                return true;
            }

            _logger.LogTrace("Not a authority signed certificate.");
            _logger.LogTrace("Server Cert RAW: {0}{1}", Environment.NewLine, Convert.ToBase64String(caCert.RawData));

            // When there is Remote Certificate Chain Error, verify the chain relation between the client and server certificates.
            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
            {
                _logger.LogTrace("Building certificate chain.");
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                // add all your extra certificate chain
                chain.ChainPolicy.ExtraStore.Add(caCert);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                _logger.LogTrace("Client Cert RAW: {0}{1}", Environment.NewLine, Convert.ToBase64String(clientCertificate.RawData));
                bool isValid = chain.Build(clientCertificate);
                _logger.LogTrace("Is Chain successfully built: {0}", isValid);
                return isValid;
            }

            // In all other cases, return false.
            _logger.LogError("SSL Certificate validation failed.");
            return false;
        }

        /// <summary>
        /// Gets access token for querying Kubernetes.
        /// </summary>
        /// <returns></returns>
        public string GetToken()
        {
            string tokenFilePath = GetTokenFilePath();
            if (!File.Exists(tokenFilePath))
            {
                throw new FileNotFoundException("Token file doesn't exist for kubernetes query.", tokenFilePath);
            }
            return File.ReadAllText(tokenFilePath);
        }

        protected abstract string GetTokenFilePath();
        protected abstract string GetCertFilePath();

        protected static string FetchQueryNamespace(string pathToNamespace)
        {
            if (string.IsNullOrEmpty(pathToNamespace))
            {
                throw new ArgumentException($"'{nameof(pathToNamespace)}' cannot be null or empty.", nameof(pathToNamespace));
            }

            if (!File.Exists(pathToNamespace))
            {
                throw new FileNotFoundException("File contains namespace does not exist.", pathToNamespace);
            }
            return File.ReadAllText(pathToNamespace);
        }

        private string GetContainerIdOrThrow()
        {
            foreach (IContainerIdProvider provider in _containerIdProviders)
            {
                if (provider.TryGetMyContainerId(out string? containerId))
                {
                    if (containerId is null)
                    {
                        throw new InvalidOperationException("Valid containerId can't be null.");
                    }
                    return containerId;
                }
            }
            throw new InvalidOperationException("Failed fetching container id.");
        }
    }
}
