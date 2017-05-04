using System;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class K8sWatcherEventArgs : EventArgs
    {
        public string EventType { get; set; }
        public string ObjectUid { get; set; }
        public string ObjectName { get; set; }
        public string ObjectKind { get; set; }
        public K8sObject Entity { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
