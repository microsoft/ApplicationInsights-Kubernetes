namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

    internal class KubeHttpClientSettingsProvider : IKubeHttpClientSettingsProvider
    {
        private string pathToToken;
        private string pathToCert;

        public Uri ServiceBaseAddress { get; private set; }
        public string QueryNamespace { get; private set; }
        public string ContainerId { get; private set; }

        public KubeHttpClientSettingsProvider(
            string pathToToken = @"/var/run/secrets/kubernetes.io/serviceaccount/token",
            string pathToCert = @"/var/run/secrets/kubernetes.io/serviceaccount/ca.crt",
            string pathToNamespace = @"/var/run/secrets/kubernetes.io/serviceaccount/namespace",
            string pathToCGroup = @"/proc/self/cgroup",
            string kubernetesServiceHost = null,
            string kubernetesServicePort = null)
        {
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
            // Environment.SetEnvironmentVariable("SSL_CERT_FILE", this.pathToCert);

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
#if DEBUG
            Console.WriteLine(Invariant($"Kubernetes base address: {baseAddress}"));
#endif
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
                ServerCertificateCustomValidationCallback = (message, certResult, chain, policyErrors) => true
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

        private void EnableCertificate(HttpClientHandler handler)
        {
            X509Certificate2 cert = new X509Certificate2(this.pathToCert);
#if DEBUG
            Console.WriteLine(cert.ToString());
#endif
            handler.UseDefaultCredentials = false;
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ClientCertificates.Add(cert);
        }
    }
}
