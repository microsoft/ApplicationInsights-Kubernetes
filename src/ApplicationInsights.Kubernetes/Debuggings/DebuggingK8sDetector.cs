using Microsoft.ApplicationInsights.Kubernetes.Utilities;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    internal class DebuggingK8sDetector : K8sDetector
    {
        public DebuggingK8sDetector() : base(null)
        {
        }

        protected override bool IsRunningInKubernetesImp()
        {
            return true;
        }
    }
}
