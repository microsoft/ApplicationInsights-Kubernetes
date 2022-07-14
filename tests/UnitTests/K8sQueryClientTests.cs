using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class K8sQueryClientTests
    {
        [Fact(DisplayName = "Constructor should throw given null KubeHttpClient")]
        public void CtorNullHttpClientThrows()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            {
                using (new K8sQueryClient(null)) { }
            });

            Assert.Equal("kubeHttpClient", ex.ParamName);
        }

        [Fact(DisplayName = "Constructor should set KubeHttpClient")]
        public void CtorSetsKubeHttpClient()
        {
            var settingsMock = new Mock<IKubeHttpClientSettingsProvider>();
            settingsMock.Setup(p => p.CreateMessageHandler()).Returns(new HttpClientHandler());
            using (KubeHttpClient httpClient = new KubeHttpClient(settingsMock.Object))
            using (K8sQueryClient target = new K8sQueryClient(httpClient))
            {
                Assert.Same(httpClient, target.KubeHttpClient);
            }
        }

        [Fact(DisplayName = "GetPodsAsync should request the target uri")]
        public async Task GetPodsAsyncHitsTheUri()
        {
            var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

            var httpClientMock = new Mock<IKubeHttpClient>();
            httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>
                {
                    Items = new List<K8sPod>
                    {
                        new K8sPod(){ Metadata = new K8sPodMetadata(){ Name="pod1" }, Status = new K8sPodStatus(){ ContainerStatuses = new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="c1" } } }},
                        new K8sPod(){ Metadata = new K8sPodMetadata(){ Name="pod2" }, Status = new K8sPodStatus(){ ContainerStatuses = new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="c2" } } }}
                    }
                }))
            };
            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            using (K8sQueryClient target = new K8sQueryClient(httpClientMock.Object))
            {
                await target.GetPodsAsync(cancellationToken: default);
            }

            httpClientMock.Verify(mock => mock.SendAsync(It.Is<HttpRequestMessage>(m => m.RequestUri.AbsoluteUri.Equals("https://baseaddress/api/v1/namespaces/queryNamespace/pods")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "GetPodsAsync should deserialize multiple pods")]
        public async Task GetPodsAsyncReturnsMultiplePods()
        {
            var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

            var httpClientMock = new Mock<IKubeHttpClient>();
            httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>
                {
                    Items = new List<K8sPod>
                    {
                        new K8sPod(){ Metadata = new K8sPodMetadata(){ Name="pod1" }, Status = new K8sPodStatus(){ ContainerStatuses = new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="c1" } } }},
                        new K8sPod(){ Metadata = new K8sPodMetadata(){ Name="pod2" }, Status = new K8sPodStatus(){ ContainerStatuses = new List<ContainerStatus>(){ new ContainerStatus() { ContainerID="c2" } } }}
                    }
                }))
            };
            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));

            using (K8sQueryClient target = new K8sQueryClient(httpClientMock.Object))
            {
                IEnumerable<K8sPod> result = await target.GetPodsAsync(cancellationToken: default);

                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.Contains(result, p => p.Metadata.Name.Equals("pod1"));
                Assert.Contains(result, p => p.Metadata.Name.Equals("pod2"));
            }
        }

        [Fact(DisplayName = "GetReplicasAsync should request the proper uri")]
        public async Task GetReplicasAsyncShouldHitsTheUri()
        {
            var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

            var httpClientMock = new Mock<IKubeHttpClient>();
            httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sReplicaSet>
                {
                    Items = new List<K8sReplicaSet>()
                }))
            };
            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            using (K8sQueryClient target = new K8sQueryClient(httpClientMock.Object))
            {
                await target.GetReplicasAsync(cancellationToken: default);
            }

            httpClientMock.Verify(mock => mock.SendAsync(It.Is<HttpRequestMessage>(m =>
                m.RequestUri.AbsoluteUri.Equals("https://baseaddress/apis/apps/v1/namespaces/queryNamespace/replicasets")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "GetReplicasAsync should deserialize multiple replicas")]
        public async Task GetReplicasAsyncAsyncShouldReturnsMultipleReplicas()
        {
            var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

            var httpClientMock = new Mock<IKubeHttpClient>();
            httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sReplicaSet>
                {
                    Items = new List<K8sReplicaSet>() {
                        new K8sReplicaSet(){ Metadata=new K8sReplicaSetMetadata(){ Name="R1" } },
                        new K8sReplicaSet(){ Metadata=new K8sReplicaSetMetadata(){ Name="R2" } }
                    }
                }))
            };
            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));

            IEnumerable<K8sReplicaSet> result;
            using (K8sQueryClient target = new K8sQueryClient(httpClientMock.Object))
            {
                result = await target.GetReplicasAsync(cancellationToken: default);
            }
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Metadata.Name.Equals("R1"));
            Assert.Contains(result, p => p.Metadata.Name.Equals("R2"));
        }

        [Fact(DisplayName = "GetDeploymentsAsync should request the proper uri")]
        public async Task GetDeploymentsAsyncShouldHitsTheUri()
        {
            var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

            var httpClientMock = new Mock<IKubeHttpClient>();
            httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>()))
            };
            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));

            using (K8sQueryClient target = new K8sQueryClient(httpClientMock.Object))
            {
                await target.GetDeploymentsAsync(cancellationToken: default);
            }
            httpClientMock.Verify(mock => mock.SendAsync(It.Is<HttpRequestMessage>(
                m => m.RequestUri.AbsoluteUri.Equals("https://baseaddress/apis/apps/v1/namespaces/queryNamespace/deployments")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "GetDeploymentsAsync should deserialize multiple deployments")]
        public async Task GetDeploymentsAsyncShouldReturnsMultipleDeployments()
        {
            var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

            var httpClientMock = new Mock<IKubeHttpClient>();
            httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sDeployment>
                {
                    Items = new List<K8sDeployment> {
                            new K8sDeployment(){ Metadata=new K8sDeploymentMetadata(){ Name="D1" } },
                            new K8sDeployment(){ Metadata=new K8sDeploymentMetadata(){ Name="D2" } }
                    }
                }))
            };
            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));

            using (K8sQueryClient target = new K8sQueryClient(httpClientMock.Object))
            {
                IEnumerable<K8sDeployment> result = await target.GetDeploymentsAsync(cancellationToken: default);
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.Contains(result, p => p.Metadata.Name.Equals("D1"));
                Assert.Contains(result, p => p.Metadata.Name.Equals("D2"));
            }
        }

        [Fact(DisplayName = "GetNodesAsync should request the proper uri")]
        public async Task GetNodesAsyncShouldHitsTheUri()
        {
            var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

            var httpClientMock = new Mock<IKubeHttpClient>();
            httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sPod>()))
            };
            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            using (K8sQueryClient target = new K8sQueryClient(httpClientMock.Object))
            {
                await target.GetNodesAsync(cancellationToken: default);
            }

            httpClientMock.Verify(mock => mock.SendAsync(It.Is<HttpRequestMessage>(m => m.RequestUri.AbsoluteUri.Equals("https://baseaddress/api/v1/nodes")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "GetNodesAsync should deserialize multiple nodes")]
        public async Task GetNodesAsyncShouldReturnsMultipleNodes()
        {
            var httpClientSettingsMock = GetKubeHttpClientSettingsProviderForTest();

            var httpClientMock = new Mock<IKubeHttpClient>();
            httpClientMock.Setup(httpClient => httpClient.Settings).Returns(httpClientSettingsMock.Object);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new K8sEntityList<K8sNode>
                {
                    Items = new List<K8sNode> {
                        new K8sNode(){ Metadata=new K8sNodeMetadata(){ Name="N1" } },
                        new K8sNode(){ Metadata=new K8sNodeMetadata(){ Name="N2" } }
                    }
                }))
            };

            httpClientMock.Setup(httpClient => httpClient.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
                response));

            using (K8sQueryClient target = new K8sQueryClient(httpClientMock.Object))
            {
                IEnumerable<K8sNode> result = await target.GetNodesAsync(cancellationToken: default);

                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.Contains(result, p => p.Metadata.Name.Equals("N1"));
                Assert.Contains(result, p => p.Metadata.Name.Equals("N2"));
            }
        }

        private Mock<IKubeHttpClientSettingsProvider> GetKubeHttpClientSettingsProviderForTest()
        {
            Uri baseUri = new Uri("https://baseAddress/");
            string queryNamespace = nameof(queryNamespace);
            string containerId = nameof(containerId);

            var httpClientSettings = new Mock<IKubeHttpClientSettingsProvider>();
            httpClientSettings.Setup(settings => settings.ServiceBaseAddress).Returns(baseUri);
            httpClientSettings.Setup(settings => settings.QueryNamespace).Returns(queryNamespace);
            httpClientSettings.Setup(settings => settings.ContainerId).Returns(containerId);

            return httpClientSettings;
        }
    }
}
