namespace Microsoft.ApplicationInsights.Kubernetes.PodInfoProviders;

/// <summary>
/// A service that provides pod name
/// </summary>
internal interface IPodNameProvider
{
    /// <summary>
    /// Tries to get pod name.
    /// </summary>
    /// <param name="podName">The output of the pod name.</param>
    /// <returns>A boolean indicate whether the fetch succeeded or not. When it is true, the pod name will be there in podName. When false, podName will be string.Empty.</returns>
    bool TryGetPodName(out string podName);
}
