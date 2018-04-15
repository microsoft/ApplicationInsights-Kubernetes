using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal abstract class KubeHttpClientSettingsBase : IKubeHttpClientSettingsProvider
    {
        protected readonly ILogger _logger;

        public KubeHttpClientSettingsBase(
            string kubernetesServiceHost,
            string kubernetesServicePort,
            ILogger<KubeHttpClientSettingsBase> logger)
        {
            _logger = Arguments.IsNotNull(logger, nameof(logger));

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
            _logger.LogDebug(Invariant($"Kubernetes base address: {baseAddress}"));
            ServiceBaseAddress = new Uri(baseAddress, UriKind.Absolute);
        }

        public string ContainerId { get; protected set; }

        public string QueryNamespace { get; protected set; }

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
                    return VerifyServerCertificate(httpRequestMessage, new CertificateVerifier(serverCert), chain, policyErrors, new CertificateVerifier(clientCert));
                }
            };

            return handler;
        }

        /// <summary>
        /// Get access token for querying Kubernetes.
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
            if (!File.Exists(pathToNamespace))
            {
                throw new FileNotFoundException("File contains namespace does not exist.", pathToNamespace);
            }
            return File.ReadAllText(pathToNamespace);
        }

        /// <summary>
        /// Verify server certificate within returned HttpResponse to prevent MITM attack.
        /// </summary>
        /// <param name="httpRequestMessage">The httpRequestMessage returned.</param>
        /// <param name="serverCertVerifier">The server certificate.</param>
        /// <param name="chain">The X509Chain.</param>
        /// <param name="policyErrors">SslPolicyErrors.</param>
        /// <param name="clientCertVerifier">Client certificate.</param>
        /// <returns>Returns true when server certificate is valid.</returns>
        internal bool VerifyServerCertificate(HttpRequestMessage httpRequestMessage, ICertificateVerifier serverCertVerifier, X509Chain chain, SslPolicyErrors policyErrors, ICertificateVerifier clientCertVerifier)
        {
            Arguments.IsNotNull(clientCertVerifier, nameof(clientCertVerifier));
            X509Certificate2 serverCert = serverCertVerifier.Certificate;
            _logger.LogDebug("Server certification custom validation callback.");
            _logger.LogTrace(httpRequestMessage?.ToString());
            _logger.LogTrace(chain?.ToString());
            _logger.LogTrace(policyErrors.ToString());
            _logger.LogTrace("ServerCert:" + Environment.NewLine + serverCert);

            try
            {
                // Verify Issuer. Issuer field is case-insensitive.
                if (!string.Equals(clientCertVerifier.Issuer, serverCertVerifier.Issuer, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError(Invariant($"Issuer are different for server certificate and the client certificate. Server Certificate Issuer: {clientCertVerifier.Issuer}, Client Certificate Issuer: {serverCertVerifier.Issuer}"));
                    return false;
                }
                else
                {
                    _logger.LogDebug(Invariant($"Issuer validation passed: {serverCertVerifier.Issuer}"));
                }

                // Server certificate is not expired.
                DateTime now = DateTime.Now;
                if (serverCertVerifier.NotBefore > now || serverCertVerifier.NotAfter.AddDays(1) <= now)
                {
                    _logger.LogError(Invariant($"Server certification is not in valid period from {serverCertVerifier.NotBefore.ToString(DateTimeFormatInfo.InvariantInfo)} until {serverCertVerifier.NotAfter.ToString(DateTimeFormatInfo.InvariantInfo)}"));
                    return false;
                }
                else
                {
                    _logger.LogDebug("Server certificate validate date verification passed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
            _logger.LogDebug("Server certification custom validation successed.");
            return true;
        }
    }
}
