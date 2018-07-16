using Microsoft.ApplicationInsights.Kubernetes.Utilities;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    public class K8sDetectorStub : K8sDetector
    {
        private bool _result;
        public K8sDetectorStub(bool result):base(null)
        {
            _result = result;
        }

        protected override bool IsRunningInKubernetesImp()
        {
            return _result;
        }
    }
}
