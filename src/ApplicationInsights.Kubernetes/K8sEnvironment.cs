using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Kubernetes.Entities;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    /// <summary>
    /// Flatten objects for application insights or other external caller to fetch K8s properties.
    /// </summary>
    internal class K8sEnvironment : IK8sEnvironment
    {
        // Property holder objects
        internal K8sPod myPod;
        internal ContainerStatus myContainerStatus;
        internal K8sReplicaSet myReplicaSet;
        internal K8sDeployment myDeployment;
        internal K8sNode myNode;

        #region Shorthands to the properties
        /// <summary>
        /// ContainerID for the current K8s entity.
        /// </summary>
        public string ContainerID { get; internal set; }

        /// <summary>
        /// Name of the container specificed in deployment spec.
        /// </summary>
        public string ContainerName => this.myContainerStatus?.Name;

        /// <summary>
        /// Name of the Pod
        /// </summary>
        public string PodName => this.myPod?.Metadata?.Name;

        /// <summary>
        /// GUID for a Pod
        /// </summary>
        public string PodID => this.myPod?.Metadata?.Uid;

        /// <summary>
        /// Labels for a pod
        /// </summary>
        public string PodLabels
        {
            get
            {
                string result = null;
                IDictionary<string, string> labelDict = myPod?.Metadata?.Labels;
                if (labelDict != null && labelDict.Count > 0)
                {
                    result = JoinKeyValuePairs(labelDict);
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the namespace for a pod
        /// </summary>
        public string PodNamespace => this.myPod?.Namespace;

        public string ReplicaSetUid => this.myReplicaSet?.Metadata?.Uid;
        public string ReplicaSetName => this.myReplicaSet?.Metadata?.Name;

        public string DeploymentUid => this.myDeployment?.Metadata?.Uid;
        public string DeploymentName => this.myDeployment?.Metadata?.Name;

        public string NodeName => this.myNode?.Metadata?.Name;
        public string NodeUid => this.myNode?.Metadata?.Uid;
        #endregion

        private static string JoinKeyValuePairs(IDictionary<string, string> dictionary)
        {
            return string.Join(",", dictionary.Select(kvp => kvp.Key + ':' + kvp.Value));
        }
    }
}
