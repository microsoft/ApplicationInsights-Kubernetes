using System.Collections.Generic;
using System.Linq;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    /// <summary>
    /// Flatten objects for application insights or other external caller to fetch K8s properties.
    /// </summary>
    internal class K8sEnvironment : IK8sEnvironment
    {
        private readonly V1Pod _pod;
        private readonly V1ContainerStatus? _containerStatus;
        private readonly V1ReplicaSet? _replicaSet;
        private readonly V1Deployment? _deployment;
        private readonly V1Node? _node;

        public K8sEnvironment(
            V1ContainerStatus? containerStatus,
            V1Pod pod,
            V1ReplicaSet? replicaSet,
            V1Deployment? deployment,
            V1Node? node
            )
        {
            _pod = pod ?? throw new System.ArgumentNullException(nameof(pod));
            ContainerID = containerStatus?.ContainerID;
            _containerStatus = containerStatus;
            _replicaSet = replicaSet;
            _deployment = deployment;
            _node = node;
        }


        #region Shorthands to the properties
        /// <summary>
        /// ContainerID for the current K8s entity.
        /// </summary>
        public string? ContainerID { get; }

        /// <summary>
        /// Name of the container specified in deployment spec.
        /// </summary>
        public string? ContainerName => _containerStatus?.Name;

        /// <summary>
        /// Name of the Pod
        /// </summary>
        public string PodName => _pod.Metadata.Name;

        /// <summary>
        /// GUID for a Pod
        /// </summary>
        public string PodID => _pod.Metadata.Uid;

        /// <summary>
        /// Labels for a pod
        /// </summary>
        public string? PodLabels
        {
            get
            {
                string? result = null;
                IDictionary<string, string>? labelDict = _pod.Metadata?.Labels;
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
        public string? PodNamespace => _pod.Metadata.NamespaceProperty;

        public string? ReplicaSetUid => _replicaSet?.Metadata?.Uid;
        public string? ReplicaSetName => _replicaSet?.Metadata?.Name;

        public string? DeploymentUid => _deployment?.Metadata?.Uid;
        public string? DeploymentName => _deployment?.Metadata?.Name;

        public string? NodeName => _node?.Metadata?.Name;
        public string? NodeUid => _node?.Metadata?.Uid;
        #endregion

        private static string JoinKeyValuePairs(IDictionary<string, string> dictionary)
        {
            return string.Join(",", dictionary.Select(kvp => kvp.Key + ':' + kvp.Value));
        }
    }
}
