using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpClientSettingsProvider : KubeHttpClientSettingsBase, IKubeHttpClientSettingsProvider
    {
        public const string CGroupPathPatternString = "cpu.+/([^/]*)$";
        public static readonly Regex CGroupPathPattern = new Regex(CGroupPathPatternString, RegexOptions.CultureInvariant | RegexOptions.Multiline);

        private readonly string _certFilePath;
        private readonly string _tokenFilePath;

        public KubeHttpClientSettingsProvider()
            : this(kubernetesServiceHost: null)
        {
        }

        public KubeHttpClientSettingsProvider(
            string pathToToken = @"/var/run/secrets/kubernetes.io/serviceaccount/token",
            string pathToCert = @"/var/run/secrets/kubernetes.io/serviceaccount/ca.crt",
            string pathToNamespace = @"/var/run/secrets/kubernetes.io/serviceaccount/namespace",
            string pathToCGroup = @"/proc/self/cgroup",
            string kubernetesServiceHost = null,
            string kubernetesServicePort = null)
            : base(kubernetesServiceHost, kubernetesServicePort)
        {
            _tokenFilePath = Arguments.IsNotNullOrEmpty(pathToToken, nameof(pathToToken));
            _certFilePath = Arguments.IsNotNullOrEmpty(pathToCert, nameof(pathToCert));
            QueryNamespace = FetchQueryNamespace(Arguments.IsNotNullOrEmpty(pathToNamespace, nameof(pathToNamespace)));
            ContainerId = FetchContainerId(pathToCGroup);
        }

        private static string FetchContainerId(string pathToCGroup)
        {
            if (!File.Exists(pathToCGroup))
            {
                throw new FileNotFoundException("File contains container id does not exist.", pathToCGroup);
            }

            string content = File.ReadAllText(pathToCGroup);
            return ParseContainerId(content);
        }

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

        protected override string GetTokenFilePath()
        {
            return _tokenFilePath;
        }

        protected override string GetCertFilePath()
        {
            return _certFilePath;
        }

        internal KubeHttpClientSettingsProvider(bool isForTesting)
            : base("http://127.0.0.1", "8001")
        {
            if (!isForTesting)
            {
                throw new InvalidOperationException("This constructor is only supposed to be used by unit tests.");
            }
        }
    }
}
