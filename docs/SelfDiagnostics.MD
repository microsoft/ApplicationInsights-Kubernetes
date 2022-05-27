# [Advanced] How to enable self diagnostics for ApplicationInsights.Kubernetes

## Enable self-diagnostics

Enabling self-diagnostics helps troubleshoot issues and determine whether the problem is with service code/configuration, or with the `ApplicationInsights.Kubernetes` package.

If problem with `ApplicationInsights.Kubernetes` is suspected, please open an [issues](https://github.com/microsoft/ApplicationInsights-Kubernetes/issues) and we will look into it.

## With 2.0.4-beta1 or above

```csharp
// LogLevel.Error is the default; Setting it to LogLevel.Trace to see detailed logs.
builder.Services.AddApplicationInsightsKubernetesEnricher(LogLevel.Trace);
```

Refer to [Program.cs](https://github.com/microsoft/ApplicationInsights-Kubernetes/blob/c8a906080f368570078d873b61f58dd742277916/dev/F5WebApi/Program.cs#L10) for a complete example.


## With versions before 2.0.4-beta1

<details>
<summary>Click to expand!</summary>

```csharp
using Microsoft.ApplicationInsights.Kubernetes.Debugging;
...
var observer = new ApplicationInsightsKubernetesDiagnosticObserver(DiagnosticLogLevel.Trace);
ApplicationInsightsKubernetesDiagnosticSource.Instance.Observable.SubscribeWithAdapter(observer);
```

Refer to [Startup.cs](https://github.com/microsoft/ApplicationInsights-Kubernetes/blob/36e31d39e4ef867fadbb3a4191f82565af3cb5b0/dev/F5WebApi/Startup.cs#L26) for a complete example.
</details>

## Log example

When the observer above is enabled, you will see diagnostic logs like this in the console:

```shell
...
[Debug] Application Insights Kubernetes injected the service successfully.
[Debug] Initialize Application Insights for Kubernetes telemetry initializer with Options:
{"InitializationTimeout":"00:02:00"}
[Debug] Application Insights for Kubernetes environment initialized.
[Trace] Inject into telemetry configuration: 60695621
[Information] KubernetesTelemetryInitializer is injected.
...
```

Or you can fetch the log by calling kubectl logs <PodName> [ContainerName]. For example:

```shell
kubectl logs x-u-service-20450933-b61w2 x-webapi
```

A similar log will be output to the terminal.

## Logging Levels

There are 6 levels of the events, each is assigned with a number like:

* None - 6
* Critical - 5
* Error - 4
* Warning - 3
* Information - 2
* Debug - 1
* Trace - 0

When minimum level is set at the creation of the observer, issues that has a number greater or equal to it will be printed. For example, if the minimum level is set to `Error`(4), logs with the level of critical or error will be printed to the console.
Set LogLevel to None to complete turn off the logs.

## About the Diagnostic Source

To find out more about the [DiagnosticSource], refer to [DiagnosticSource Users Guide](https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md#diagnosticsource-users-guide).

If you want to write another diagnostic source observer, here's the basic information:

The DiagnosticSource used in Application Insights is named `ApplicationInsightsKubernetesDiagnosticSource`.

There happen to be 5 predefined events, named after the levels - "Critical", "Error", "Warning", "Information", "Debug" and "Trace".
