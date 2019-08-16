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

* Optionally, if you prefer to output the log to the console simultaneously, add the following package as well:

    ```shell
    dotnet add package Microsoft.Extensions.Logging.Console
    ```

    The project file will be updated, similar to [BasicConsoleAppILogger.csproj](./BasicConsoleAppILogger.csproj).

* Replace the code in [Program.cs](Program.cs).

* To build a docker image, refer to [Dockerfile](./Dockerfile).

* To deploy the docker image to a Kubernetes cluster, refer to [k8s.yaml](./k8s.yaml).

## References

* [ApplicationInsightsLoggerProvider](https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger).
