using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    public class K8sPodWatcherTests
    {
        [Fact(DisplayName = "K8sPodWatcher can be successfully deserialized from json string.")]
        public async Task ShouldBeAbleToDeserizeStringToObjectAsync()
        {
            using (FileStream stream = new FileStream("WatchTestData.json", FileMode.Open))
            using (StreamReader sr = new StreamReader(stream))
            {
                string json = await sr.ReadToEndAsync();

                K8sWatcherObject<K8sPod, K8sPodMetadata> target = JsonConvert.DeserializeObject<K8sWatcherObject<K8sPod, K8sPodMetadata>>(json);

                Assert.NotNull(target);
                Assert.Equal("ADDED", target.EventType);

                Assert.NotNull(target.EventObject);
                Assert.NotNull(target.EventObject.Metadata);
                Assert.Equal("saars-u-service-2503151269-gvzx1", target.EventObject.Metadata.Name);
            }
        }
    }
}
