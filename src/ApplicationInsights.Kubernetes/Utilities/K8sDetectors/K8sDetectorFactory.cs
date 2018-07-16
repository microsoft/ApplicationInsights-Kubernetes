namespace Microsoft.ApplicationInsights.Kubernetes.Utilities
{
    internal sealed class K8sDetectorFactory
    {
        private K8sDetectorFactory() { }

        public static K8sDetectorFactory Instance { get; } = new K8sDetectorFactory();

#pragma warning disable CA1822 // Mark members as static
        public K8sDetector CreateDetector() => new K8sDetectorLinux(new K8sDetectorWindows(null));
#pragma warning restore CA1822 // Mark members as static
    }
}
