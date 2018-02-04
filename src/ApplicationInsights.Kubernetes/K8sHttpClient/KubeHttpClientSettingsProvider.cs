namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Logging;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

    internal class KubeHttpClientSettingsProvider : IKubeHttpClientSettingsProvider
    {
        public const string CGroupPathPatternString = "cpu.+/([^/]*)$";
        public static readonly Regex CGroupPathPattern = new Regex(CGroupPathPatternString, RegexOptions.CultureInvariant | RegexOptions.Multiline);

        private string _pathToToken;
        private string _pathToCert;
        private readonly ILogger _logger;

        public Uri ServiceBaseAddress { get; private set; }
        public string QueryNamespace { get; private set; }
        public string ContainerId { get; private set; }

        internal static string ParseContainerId(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                MatchCollection matches = CGroupPathPattern.Matches(content);
                if (matches.Count >= 1 && matches[0].Groups.Count >= 2)
                {
                    return matches[0].Groups[1].Value;
                }
            }

            throw new InvalidCastException(Invariant($"Can't figure out container id. Input: {content}. Pattern: {CGroupPathPatternString}"));
        }

        internal KubeHttpClientSettingsProvider(bool isForTesting)
        {
            if (!isForTesting)
            {
                throw new InvalidOperationException("This constructor is only supposed to be used by unit tests.");
            }
        }

        public KubeHttpClientSettingsProvider(ILogger<KubeHttpClientSettingsProvider> logger)
            : this(logger, kubernetesServiceHost: null)
        {
        }

        public KubeHttpClientSettingsProvider(
            ILogger<KubeHttpClientSettingsProvider> logger,
            string pathToToken = @"/var/run/secrets/kubernetes.io/serviceaccount/token",
            string pathToCert = @"/var/run/secrets/kubernetes.io/serviceaccount/ca.crt",
            string pathToNamespace = @"/var/run/secrets/kubernetes.io/serviceaccount/namespace",
            string pathToCGroup = @"/proc/self/cgroup",
            string kubernetesServiceHost = null,
            string kubernetesServicePort = null)
        {
            this._logger = Arguments.IsNotNull(logger, nameof(logger));

            if (string.IsNullOrEmpty(pathToToken))
            {
                throw new ArgumentNullException(nameof(pathToToken));
            }
            this._pathToToken = pathToToken;

            if (string.IsNullOrEmpty(pathToCert))
            {
                throw new ArgumentNullException(nameof(pathToCert));
            }
            this._pathToCert = pathToCert;

            if (string.IsNullOrEmpty(pathToNamespace))
            {
                throw new ArgumentNullException(nameof(pathToNamespace));
            }
            this.QueryNamespace = FetchQueryNamespace(pathToNamespace);

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
            this.ContainerId = FetchContainerId(pathToCGroup);
            string baseAddress = Invariant($"https://{kubernetesServiceHost}:{kubernetesServicePort}/");
            this._logger?.LogDebug(Invariant($"Kubernetes base address: {baseAddress}"));
            ServiceBaseAddress = new Uri(baseAddress, UriKind.Absolute);
        }

        public string GetToken()
        {
            if (!File.Exists(this._pathToToken))
            {
                throw new FileNotFoundException("Token file doesn't exist for kubernetes query.", this._pathToToken);
            }

            string token = null;
            using (FileStream fileStream = File.OpenRead(_pathToToken))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                token = reader.ReadToEnd();
            }

            return token;
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
        public bool VerifyServerCertificate(HttpRequestMessage httpRequestMessage, ICertificateVerifier serverCertVerifier, X509Chain chain, SslPolicyErrors policyErrors, ICertificateVerifier clientCertVerifier)
        {
            Arguments.IsNotNull(clientCertVerifier, nameof(clientCertVerifier));
            X509Certificate2 serverCert = serverCertVerifier.Certificate;
            _logger?.LogDebug("Server certification custom validation callback.");
            _logger?.LogTrace(httpRequestMessage?.ToString());
            _logger?.LogTrace(chain?.ToString());
            _logger?.LogTrace(policyErrors.ToString());
            _logger?.LogTrace("ServerCert:" + Environment.NewLine + serverCert);

            try
            {
                // Verify Issuer. Issuer field is case-insensitive.
                if (!string.Equals(clientCertVerifier.Issuer, serverCertVerifier.Issuer, StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogError(Invariant($"Issuer are different for server certificate and the client certificate. Server Certificate Issuer: {clientCertVerifier.Issuer}, Client Certificate Issuer: {serverCertVerifier.Issuer}"));
                    return false;
                }
                else
                {
                    _logger?.LogDebug(Invariant($"Issuer validation passed: {serverCertVerifier.Issuer}"));
                }

                // Server certificate is not expired.
                DateTime now = DateTime.Now;
                if (serverCertVerifier.NotBefore > now || serverCertVerifier.NotAfter.AddDays(1) <= now)
                {
                    _logger?.LogError(Invariant($"Server certification is not in valid period from {serverCertVerifier.NotBefore.ToString(DateTimeFormatInfo.InvariantInfo)} until {serverCertVerifier.NotAfter.ToString(DateTimeFormatInfo.InvariantInfo)}"));
                    return false;
                }
                else
                {
                    _logger?.LogDebug("Server certificate validate date verification passed.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.ToString());
                return false;
            }
            _logger?.LogDebug("Server certification custom validation successed.");
            return true;
        }

        public HttpMessageHandler CreateMessageHandler()
        {
            if (!File.Exists(this._pathToCert))
            {
                throw new FileNotFoundException("Certificate file is required to access kubernetes API.", this._pathToCert);
            }

            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, serverCert, chain, policyErrors) =>
                {
                    X509Certificate2 clientCert = new X509Certificate2(this._pathToCert);
                    return VerifyServerCertificate(httpRequestMessage, new CertificateVerifier(serverCert), chain, policyErrors, new CertificateVerifier(clientCert));
                }
            };

            return handler;
        }

        private static string FetchQueryNamespace(string pathToNamespace)
        {
            if (!File.Exists(pathToNamespace))
            {
                throw new FileNotFoundException("File contains namespace does not exist.", pathToNamespace);
            }

            string result = null;
            using (FileStream fileStream = File.OpenRead(pathToNamespace))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private static string FetchContainerId(string pathToCGroup)
        {
            string content = null;
            if (!File.Exists(pathToCGroup))
            {
                throw new FileNotFoundException("File contains container id does not exist.", pathToCGroup);
            }

            using (FileStream fileStream = File.OpenRead(pathToCGroup))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                content = reader.ReadToEnd();
            }

            return ParseContainerId(content);
        }
    }
}
