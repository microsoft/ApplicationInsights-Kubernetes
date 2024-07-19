# Microsoft Application Insights for Kubernetes

[![Nuget](https://img.shields.io/nuget/v/Microsoft.ApplicationInsights.Kubernetes)](https://www.nuget.org/packages/Microsoft.ApplicationInsights.Kubernetes/)
[![Downloads](https://img.shields.io/nuget/dt/Microsoft.ApplicationInsights.Kubernetes)](https://www.nuget.org/packages/Microsoft.ApplicationInsights.Kubernetes/)

Application Insights for Kubernetes enhances telemetries with K8s properties, works for .NET Core/.NET 6 applications in Kubernetes clusters.

![Screenshot for Application Insights for Kubernetes enhanced telemetry](./docs/TelemetryEnhancement.png)

> ⚠️ `Microsoft Application Insights for Kubernetes` (this library) is designed to improve the functionality of [Microsoft Application Insights](https://github.com/Microsoft/ApplicationInsights-aspnetcore). While **it is possible** to run Application Insights on a Kubernetes cluster **without** this library, it allows for Kubernetes-specific properties such as Pod-Name and Deployment to be included in all telemetry entries. Additionally, the library ensures that appropriate values are assigned, enabling advanced features like the Application Map to display multiple microservices or roles on a single Application Insights map.

## Continuous Integration Status

| Rolling Build                                                                                                                           | Nightly Build                                                                                                                           |
| --------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
| ![Rolling-Build Status](https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/5974/badge) | ![Nightly-Build Status](https://devdiv.visualstudio.com/_apis/public/build/definitions/0bdbc590-a062-4c3f-b0f6-9383f67865ee/5976/badge) |

## Get Started

This is a quick guide for **ASP.NET Core** projects. If you want to run it **in a worker**, see the instructions in the **[Walk-through](#walk-through-with-example-code)** section.

### Prerequisite

* [Application Insights for ASP.NET Core](https://github.com/Microsoft/ApplicationInsights-aspnetcore)
* [Docker Containers](https://www.docker.com/)
* [Kubernetes](https://kubernetes.io/)

> ⚠️ For RBAC-enabled Kubernetes, make sure [permissions are configured](./docs/configure-rbac-permissions.md).

### Instrument an ASP.NET Core application

1. Add references to **Application Insights SDK** and **Application Insights for Kubernetes** by running:

    ```shell
    dotnet add package Microsoft.ApplicationInsights.AspNetCore
    dotnet add package Microsoft.ApplicationInsights.Kubernetes
    ```

2. Enable **Application Insights** and **Application Insights for Kubernetes Enricher** in `Startup.cs` / `Program.cs`:

    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddApplicationInsightsTelemetry("----Your Application Insights Instrumentation Key ----");
        services.AddApplicationInsightsKubernetesEnricher();
        ...
    }
    ```

3. Build the application in containers, then deploy the container with Kubernetes.

**Notes:** Those steps are not considered the best practice to set the instrumentation key for application insights. Refer to [Enable Application Insights server-side telemetry](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core#enable-application-insights-server-side-telemetry-without-visual-studio) for various options. Also, consider deploying [Kubernetes Secrets](https://kubernetes.io/docs/concepts/configuration/secret/) to secure it. Refer to [Deploy the application in Kubernetes
](examples/ZeroUserCodeLightup.Net6/README.md#deploy-the-application-in-kubernetes) for an example.

### Walk-through with example code

* [Enable Application Insights for Kubernetes on WebAPI (Controller based or Minimal API)](./examples/WebAPI.Net6/Readme.md).
* [Enable Application Insights Kubernetes in **worker project**](./examples/WorkerExample/Readme.md).

### Customize Configurations

Customize configurations are supported. There are several ways to customize the settings. For example:

1. Using code:

    ```csharp
    services.AddApplicationInsightsKubernetesEnricher(option=> {
        option.InitializationTimeout = TimeSpan.FromSeconds(15);
    });
    ```

2. Using `appsettings.json`:

    ```jsonc
    {
        "Logging": {
            // ...
        },
        // Adding the following section to set the timeout to 15 seconds
        "AppInsightsForKubernetes": {
            "InitializationTimeout": "00:00:15"
        }
    }
    ```

3. Using environment variables:

    ```shell
    AppInsightsForKubernetes__InitializationTimeout=3.1:12:15.34
    ```

    All the related configurations have to be put in a section named `AppInsightsForKubernetes`. The supported keys/values are listed below:

    | Key                        | Value/Types | Default Value | Description                                                                                  |
    | -------------------------- | ----------- | ------------- | -------------------------------------------------------------------------------------------- |
    | InitializationTimeout      | TimeSpan    | 00:02:00      | Maximum time to wait for spinning up the container. Accepted format: [d.]hh:mm:ss[.fffffff]. |
    | ClusterInfoRefreshInterval | TimeSpan    | 00:10:00      | Get or sets how frequent to refresh the Kubernetes cluster properties.         |
    | ~~DisablePerformanceCounters~~ | ~~Boolean~~     | ~~false~~         | Deprecated. Sets to true to avoid adding performance counter telemetry initializer.          |

> Tips: Refer to [AppInsightsForKubernetesOptions.cs](./src/ApplicationInsights.Kubernetes/Extensions/AppInsightsForKubernetesOptions.cs) for the latest customization supported.

The configuration uses the ASP.NET Core conventions. Refer to [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1) for more information.

### Verify the cluster configuration (Linux Container only)

Use the [troubleshooting image](https://github.com/Microsoft/ApplicationInsights-Kubernetes/tree/develop/troubleshooting) to verify the cluster is properly configured.

### Learn more

* To build a container for Kubernetes that have Application Insights baked in for the existing applications, please refer to the example of [Zero Code light up](https://github.com/Microsoft/ApplicationInsights-Kubernetes/tree/develop/examples/ZeroUserCodeLightup).
* To enable diagnostic logs when Application Insights for Kubernetes doesn't work as expected, reference [How to enable self diagnostics for ApplicationInsights.Kubernetes](docs/SelfDiagnostics.MD).
* Still want more? Read the [Wikis](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki).

### Next step

Profile your application for performance improvement using [Application Insights Profiler for Linux](https://github.com/Microsoft/ApplicationInsights-Profiler-AspNetCore).

## Contributing

### Report issues

Please file bug, discussion or any other interesting topics in [issues](https://github.com/Microsoft/ApplicationInsights-Kubernetes/issues) on GitHub.

### Troubleshooting

Read the [FAQ](https://github.com/microsoft/ApplicationInsights-Kubernetes/wiki/FAQ) for common issues. When Microsoft.ApplicationInsights.Kubernetes doesn't work properly, you can turn on self-diagnostics to see the traces in Kubernetes' logs. Refer to [How to enable self diagnostics for ApplicationInsights.Kubernetes](./docs/SelfDiagnostics.MD) for instructions.

### Developing

Please refer the [Develop Guide](https://github.com/Microsoft/ApplicationInsights-Kubernetes/wiki/Development-Guide).

---
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
