#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpClientSettingsProvider : KubeHttpClientSettingsBase, IKubeHttpClientSettingsProvider
    {
        public const string CGroupPathPatternString = "cpu.+/([^/]*)$";
        public static readonly Regex CGroupPathPattern = new Regex(CGroupPathPatternString, RegexOptions.CultureInvariant | RegexOptions.Multiline, TimeSpan.FromSeconds(1));

        private readonly string _certFilePath;
        private readonly string _tokenFilePath;

        public KubeHttpClientSettingsProvider(IEnumerable<IContainerIdProvider> containerIdProviders)
            : this(containerIdProviders, kubernetesServiceHost: null)
        {
        }

        public KubeHttpClientSettingsProvider(
            IEnumerable<IContainerIdProvider> containerIdProviders,
            string pathToToken = @"/var/run/secrets/kubernetes.io/serviceaccount/token",
            string pathToCert = @"/var/run/secrets/kubernetes.io/serviceaccount/ca.crt",
            string pathToNamespace = @"/var/run/secrets/kubernetes.io/serviceaccount/namespace",
            string? kubernetesServiceHost = null,
            string? kubernetesServicePort = null)
            : base(kubernetesServiceHost, kubernetesServicePort, containerIdProviders)
        {
            _tokenFilePath = Arguments.IsNotNullOrEmpty(pathToToken, nameof(pathToToken));
            _certFilePath = Arguments.IsNotNullOrEmpty(pathToCert, nameof(pathToCert));
            QueryNamespace = FetchQueryNamespace(pathToNamespace);
        }

        public override string QueryNamespace { get; }

        protected override string GetTokenFilePath()
        {
            return _tokenFilePath;
        }

        protected override string GetCertFilePath()
        {
            return _certFilePath;
        }
    }
}
