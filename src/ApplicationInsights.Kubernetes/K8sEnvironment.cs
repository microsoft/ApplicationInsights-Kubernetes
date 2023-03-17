using System;
using System.Collections.Generic;
using System.Linq;
using k8s.Models;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    /// <summary>
    /// An immutable flat object for Application Insights or other external caller to fetch K8s properties.
    /// </summary>
    internal record K8sEnvironment : IK8sEnvironment
    {
        public K8sEnvironment(
            V1ContainerStatus? containerStatus,
            V1Pod pod,
            V1ReplicaSet? replicaSet,
            V1Deployment? deployment,
            V1Node? node)
        {
            if (pod is null)
            {
                throw new ArgumentNullException(nameof(pod));
            }

            ContainerID = containerStatus?.ContainerID;
            ContainerName = containerStatus?.Name;
            ImageName = containerStatus?.Image;

            PodName = pod.Metadata.Name;
            PodID = pod.Metadata.Uid;
            PodLabels = CreateLabels(pod);
            PodNamespace = pod.Metadata.NamespaceProperty;

            ReplicaSetUid = replicaSet?.Metadata?.Uid;
            ReplicaSetName = replicaSet?.Metadata?.Name;

            DeploymentUid = deployment?.Metadata?.Uid;
            DeploymentName = deployment?.Metadata?.Name;

            NodeName = node?.Metadata?.Name;
            NodeUid = node?.Metadata?.Uid;
        }


        /// <summary>
        /// ContainerID for the current K8s entity.
        /// </summary>
        public string? ContainerID { get; }

        /// <summary>
        /// Name of the container specified in deployment spec.
        /// </summary>
        public string? ContainerName { get; }

        /// <summary>
        /// Name of the image specified in deployment spec.
        /// </summary>
        public string? ImageName { get; }

        /// <summary>
        /// Name of the Pod
        /// </summary>
        public string PodName { get; }

        /// <summary>
        /// GUID for a Pod
        /// </summary>
        public string PodID { get; }

        /// <summary>
        /// Labels for a pod.
        /// </summary>
        public string? PodLabels { get; }

        /// <summary>
        /// Gets the namespace for a pod
        /// </summary>
        public string? PodNamespace { get; }

        public string? ReplicaSetUid { get; }
        public string? ReplicaSetName { get; }

        public string? DeploymentUid { get; }
        public string? DeploymentName { get; }

        public string? NodeName { get; }
        public string? NodeUid { get; }

        private static string JoinKeyValuePairs(IDictionary<string, string> dictionary)
        {
            return string.Join(",", dictionary.Select(kvp => kvp.Key + ':' + kvp.Value));
        }

        private static string CreateLabels(V1Pod pod)
        {
            IDictionary<string, string>? labelDict = pod.Metadata?.Labels;
            if (labelDict != null && labelDict.Count > 0)
            {
                return JoinKeyValuePairs(labelDict);
            }
            return string.Empty;
        }
    }
}
