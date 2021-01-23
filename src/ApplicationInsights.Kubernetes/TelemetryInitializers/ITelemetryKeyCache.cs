namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface ITelemetryKeyCache
    {
        string GetProcessedKey(string originalKey);
    }
}