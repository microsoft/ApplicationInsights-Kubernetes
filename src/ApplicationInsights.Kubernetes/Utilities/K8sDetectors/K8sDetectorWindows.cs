using System.IO;

namespace Microsoft.ApplicationInsights.Kubernetes.Utilities
{
    internal sealed class K8sDetectorWindows : K8sDetector
    {
        public K8sDetectorWindows(K8sDetector nextDetector) : base(nextDetector)
        {
        }

        protected override bool IsRunningInKubernetesImp()
        {
            return Directory.Exists(@"C:\var\run\secrets\kubernetes.io");
        }
    }
}
