using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ApplicationInsights.Kubernetes.Utilities;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T>? target)
        => target ?? Enumerable.Empty<T>();
}
