namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Logging;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

    internal class KubeHttpClientSettingsProvider : IKubeHttpClientSettingsProvider
    {
        private string pathToToken;
        private string pathToCert;
        private ILogger<KubeHttpClientSettingsProvider> logger;
        private string expectedSubjectAltName = null;

        public Uri ServiceBaseAddress { get; private set; }
        public string QueryNamespace { get; private set; }
        public string ContainerId { get; private set; }

        public KubeHttpClientSettingsProvider(
            ILoggerFactory loggerFactory,
            string pathToToken = @"/var/run/secrets/kubernetes.io/serviceaccount/token",
            string pathToCert = @"/var/run/secrets/kubernetes.io/serviceaccount/ca.crt",
            string pathToNamespace = @"/var/run/secrets/kubernetes.io/serviceaccount/namespace",
            string pathToCGroup = @"/proc/self/cgroup",
            string kubernetesServiceHost = null,
            string kubernetesServicePort = null)
        {
            this.logger = loggerFactory?.CreateLogger<KubeHttpClientSettingsProvider>();

            if (string.IsNullOrEmpty(pathToToken))
            {
                throw new ArgumentNullException(nameof(pathToToken));
            }
            this.pathToToken = pathToToken;

            if (string.IsNullOrEmpty(pathToCert))
            {
                throw new ArgumentNullException(nameof(pathToCert));
            }
            this.pathToCert = pathToCert;

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
            else
            {
                this.expectedSubjectAltName = kubernetesServiceHost;
            }

            kubernetesServicePort = kubernetesServicePort ?? Environment.GetEnvironmentVariable(@"KUBERNETES_SERVICE_PORT");
            if (string.IsNullOrEmpty(kubernetesServicePort))
            {
                throw new NullReferenceException("Kubernetes service port is not set.");
            }
            this.ContainerId = FetchContainerId(pathToCGroup);
            string baseAddress = Invariant($"https://{kubernetesServiceHost}:{kubernetesServicePort}/");
            this.logger?.LogDebug(Invariant($"Kubernetes base address: {baseAddress}"));
            ServiceBaseAddress = new Uri(baseAddress, UriKind.Absolute);
        }

        public string GetToken()
        {
            if (!File.Exists(this.pathToToken))
            {
                throw new FileNotFoundException("Token file doesn't exist for kubernetes query.", this.pathToToken);
            }

            string token = null;
            using (FileStream fileStream = File.OpenRead(pathToToken))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                token = reader.ReadToEnd();
            }

            return token;
        }

        public HttpMessageHandler CreateMessageHandler()
        {
            if (!File.Exists(this.pathToCert))
            {
                throw new FileNotFoundException("Certificate file is required to access kubernetes API.", this.pathToCert);
            }

            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, serverCert, chain, policyErrors) =>
                {
                    logger?.LogDebug(Invariant($"RequestUri: {httpRequestMessage.RequestUri.ToString()}"));
                    logger?.LogDebug(Invariant($"Policy Errors: {policyErrors}"));

                    logger?.LogDebug("Server certification custom validation callback.");

                    logger?.LogDebug("ServerCert:" + Environment.NewLine + serverCert);
                    X509Certificate2 clientCert = null;
                    try
                    {
                        clientCert = new X509Certificate2(this.pathToCert);
                        if (clientCert == null)
                        {
                            logger?.LogError(Invariant($"Client certificate does not exist at: {this.pathToCert}"));
                        }
                        else
                        {
                            logger?.LogDebug("ClientCert:" + Environment.NewLine + clientCert);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex.ToString());
                        return false;
                    }

                    // Issuer field is case-insensitive.
                    if (!string.Equals(clientCert.Issuer, serverCert.Issuer, StringComparison.OrdinalIgnoreCase))
                    {
                        logger?.LogError(Invariant($"Issuer are different for server certificate and the client certificate. Server Certificate Issuer: {clientCert.Issuer}, Client Certificate Issuer: {serverCert.Issuer}"));
                        return false;
                    }
                    else
                    {
                        logger.LogDebug(Invariant($"Issuer validation passed: {serverCert.Issuer}"));
                    }

                    // Server certificate must in effective for now.
                    DateTime now = DateTime.Now;
                    if (serverCert.NotBefore > now || serverCert.NotAfter < now)
                    {
                        logger?.LogError(Invariant($"Server certification is not in valid period from {serverCert.NotBefore.ToString(DateTimeFormatInfo.InvariantInfo)} until {serverCert.NotAfter.ToString(DateTimeFormatInfo.InvariantInfo)}"));
                        return false;
                    }
                    else
                    {
                        logger?.LogDebug("Server certificate validate date verification passed.");
                    }

                    // SubjectAltName should match the host name
                    string serverSubjectAltName = serverCert.GetNameInfo(X509NameType.UpnName, false);
                    string clientSubjectAltName = clientCert.GetNameInfo(X509NameType.UpnName, false);
                    logger?.LogDebug(Invariant($"Client subject alt name: {clientSubjectAltName}"));
                    if (!string.Equals(this.expectedSubjectAltName, serverSubjectAltName, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.LogError(Invariant($"SubjectAltName doesn't match. Actual:{serverSubjectAltName} Expected:{this.expectedSubjectAltName}"));
                        return false;
                    }
                    else
                    {
                        logger.LogDebug(Invariant($"SubjectAltName matched: {serverSubjectAltName}"));
                    }

                    logger?.LogDebug("Server certification custom validation successed.");
                    return true;
                }
            };

            // TODO: When time certificate can be used, remove the callback for the validation.
            // EnableCertificate(handler);
            return handler;
        }

        private string FetchQueryNamespace(string pathToNamespace)
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

        private string FetchContainerId(string pathToCGroup)
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

            string result = null;
            string pattern = "cpu.+docker/(.*)$";
            Regex regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(content);
            if (matches.Count >= 1 && matches[0].Groups.Count >= 2)
            {
                result = matches[0].Groups[1].Value;
            }
            else
            {
                throw new InvalidCastException(Invariant($"Can't figure out docker id. Input: {content}. Pattern: {pattern}"));
            }
            return result;
        }
    }
}
