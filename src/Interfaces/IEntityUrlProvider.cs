namespace Microsoft.ApplicationInsights.Kubernetes.Interfaces
{
    public interface IEntityUrlProvider
    {
        string GetRelativePathForGet(string queryNamespace);

        string GetRelativePathForWatch(string queryNamespace);
    }
}
