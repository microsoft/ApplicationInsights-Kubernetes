# Enable Application Insights for Kubernetes in .NET Core Console Application

The following code shows a sample console application that's configured to send ILogger traces to Application Insights with Kubernetes enricher.

* Create a console application

    ```shell
    dotnet new console
    ```

* Add packages

    ```shell
    dotnet add package Microsoft.Extensions.DependencyInjection
    dotnet add package Microsoft.Extensions.Logging.ApplicationInsights
    dotnet add package Microsoft.ApplicationInsights.Kubernetes
    ```

* Replace the code in [Program.cs](Program.cs).

## References

* [ApplicationInsightsLoggerProvider](https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger).
