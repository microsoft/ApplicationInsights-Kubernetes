namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal interface ITelemetryKeyCache
    {
        string GetTelemetryProcessedKey(string originalKey);
    }
}