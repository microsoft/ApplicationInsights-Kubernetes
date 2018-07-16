namespace Microsoft.ApplicationInsights.Kubernetes.Utilities
{
    /// <summary>
    /// Detects if the application is running in containers managed by Kubernetes.
    /// </summary>
    public abstract class K8sDetector
    {
        K8sDetector _nextDetector;

        public K8sDetector(K8sDetector nextDetector)
        {
            _nextDetector = nextDetector;
        }

        /// <summary>
        /// Returns true when the application is running in containers managed by Kubernetes.
        /// </summary>
        /// <returns></returns>
        public bool IsRunningInKubernetes()
        {
            return IsRunningInKubernetesImp() || (_nextDetector?.IsRunningInKubernetes() ?? false);
        }

        protected abstract bool IsRunningInKubernetesImp();
    }
}
