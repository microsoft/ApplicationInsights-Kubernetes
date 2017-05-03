using System;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal class K8sWatcherEventArgs : EventArgs
    {
        public string ObjectUid { get; set; }
        public string EventType { get; set; }
    }
}
