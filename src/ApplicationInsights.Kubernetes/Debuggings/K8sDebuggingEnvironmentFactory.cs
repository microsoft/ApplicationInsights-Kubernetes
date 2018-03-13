using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Entities;

namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    internal class K8sDebuggingEnvironmentFactory : IK8sEnvironmentFactory
    {
        public Task<K8sEnvironment> CreateAsync(TimeSpan timeout)
        {
            return Task.FromResult(new K8sEnvironment()
            {
                ContainerID = KubeHttpDebuggingClientSettings.FakeContainerId,
                myContainerStatus = new ContainerStatus()
                {
                    ContainerID = KubeHttpDebuggingClientSettings.FakeContainerId,
                    Image = nameof(ContainerStatus.Image),
                    ImageID = nameof(ContainerStatus.ImageID),
                    Name = nameof(ContainerStatus.Name),
                    Ready = true,
                },
                myDeployment = new K8sDeployment()
                {
                    Metadata = new K8sDeploymentMetadata()
                    {
                        Labels = new Dictionary<string, string>() { { "app", "stub" } },
                        Name = nameof(K8sDeploymentMetadata.Name),
                        Uid = nameof(K8sDeploymentMetadata.Uid),
                    },
                    Spec = new K8sDeploymentSpec()
                    {
                        Selector = new Selector()
                        {
                            MatchLabels = new Dictionary<string, string>() { { "app", "stub" } },
                        },
                    },
                },
                myNode = new K8sNode()
                {
                    Metadata = new K8sNodeMetadata()
                    {
                        Labels = new Dictionary<string, string>() { { "app", "stub" } },
                        Name = nameof(K8sNodeMetadata.Name),
                        Uid = nameof(K8sNodeMetadata.Uid),
                    },
                    Status = new K8sNodeStatus()
                    {
                    },
                },
                myPod = new K8sPod()
                {
                    Metadata = new K8sPodMetadata()
                    {
                        Uid = "StubPodId",
                        Name = "StubPodName",
                        Labels = new Dictionary<string, string>() { { "app", "stub" } },
                    }
                },
                myReplicaSet = new K8sReplicaSet()
                {
                    Metadata = new K8sReplicaSetMetadata()
                    {
                        Name = "StubReplicaName",
                    }
                }
            });
        }
    }
}
