using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class KubeHttpSettingsWinContainerProvider : KubeHttpClientSettingsBase, IKubeHttpClientSettingsProvider
    {
        private readonly string _tokenFilePath;
        private readonly string _certFilePath;
        private readonly string _namespaceFilePath;

        public KubeHttpSettingsWinContainerProvider(
            ILogger<KubeHttpSettingsWinContainerProvider> logger,
            string serviceAccountFolder = @"C:\var\run\secrets\kubernetes.io\serviceaccount",
            string tokenFileName = "token",
            string certFileName = "ca.crt",
            string namespaceFileName = "namespace",
            string kubernetesServiceHost = null,
            string kubernetesServicePort = null)
            : base(kubernetesServiceHost, kubernetesServicePort, logger)
        {
            // Container id won't be fetched for windows container.
            ContainerId = null;
            DirectoryInfo serviceAccountDirectory =
                new DirectoryInfo(Arguments.IsNotNullOrEmpty(serviceAccountFolder, nameof(serviceAccountFolder)));
            foreach (FileSystemInfo fileSystemInfo in serviceAccountDirectory.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
            {
                // Per current symbolic linking settings, reading the file directly will fail.
                if (!fileSystemInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) && fileSystemInfo is FileInfo fileInfo)
                {
                    string fileName = fileInfo.Name;
                    if (fileName.Equals(Arguments.IsNotNullOrEmpty(tokenFileName, nameof(tokenFileName)), StringComparison.OrdinalIgnoreCase))
                    {
                        _tokenFilePath = fileInfo.FullName;
                        _logger.LogDebug($"Found token file paht: {_tokenFilePath}");
                    }
                    else if (fileName.Equals(Arguments.IsNotNullOrEmpty(certFileName, nameof(certFileName)), StringComparison.OrdinalIgnoreCase))
                    {
                        _certFilePath = fileInfo.FullName;
                        _logger.LogDebug($"Found certificate file path: {_certFilePath}");
                    }
                    else if (fileName.Equals(Arguments.IsNotNullOrEmpty(namespaceFileName, nameof(namespaceFileName)), StringComparison.OrdinalIgnoreCase))
                    {
                        _namespaceFilePath = fileInfo.FullName;
                        _logger.LogDebug($"Found namespace file path: {_namespaceFilePath}");
                    }
                }
            }
            QueryNamespace = FetchQueryNamespace(_namespaceFilePath);
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            HttpMessageHandler handler = base.CreateMessageHandler();
            return handler;
        }

        protected override string GetTokenFilePath()
        {
            _logger.LogDebug($"Token file path: {_tokenFilePath}");
            return _tokenFilePath;
        }

        protected override string GetCertFilePath()
        {
            return _certFilePath;
        }
    }
}
