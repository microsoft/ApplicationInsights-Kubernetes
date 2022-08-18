using System.Collections.Generic;
using System.Linq;
using k8s;

namespace Microsoft.ApplicationInsights.Kubernetes.Utilities;

internal static class K8sItemsExtensions
{
    /// <summary>
    /// Returns IItems{T} as an enumerable. When the instance of IItems{T} or its Items property is null, returns
    /// Enumerable.Empty{T}.
    /// </summary>
    public static IEnumerable<T> AsEnumerable<T>(this IItems<T>? itemsHolder)
        => itemsHolder?.Items is null ? Enumerable.Empty<T>() : itemsHolder.Items;
}
