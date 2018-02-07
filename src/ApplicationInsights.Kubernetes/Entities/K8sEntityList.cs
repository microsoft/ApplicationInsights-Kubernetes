using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.ApplicationInsights.Kubernetes.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class K8sEntityList<TEntity> : IEnumerable<TEntity>
    {
        [JsonProperty("items")]
        public IEnumerable<TEntity> Items { get; set; }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
