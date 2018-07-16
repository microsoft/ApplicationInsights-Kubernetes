using System.IO;

namespace Microsoft.ApplicationInsights.Kubernetes.Utilities
{
    internal sealed class K8sDetectorLinux : K8sDetector
    {
        public K8sDetectorLinux(K8sDetector nextDetector) : base(nextDetector)
        {
        }

        protected override bool IsRunningInKubernetesImp()
        {
            return Directory.Exists(@"/var/run/secrets/kubernetes.io");
        }
    }
}
