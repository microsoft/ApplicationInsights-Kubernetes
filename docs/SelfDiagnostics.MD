# [Advanced] How to enable self diagnostics for ApplicationInsights.Kubernetes

## Enable self-diagnostics

We understand ApplicationInsights.Kubernetes is far from perfect. Enabling self-diagnostics will help find [issues](https://github.com/microsoft/ApplicationInsights-Kubernetes/issues) easier. Please feel free to open issues and we will be glad to look into them.

Application Insights for Kubernetes is instrumented with DiagnosticsSource. To see the logs, create a diagnostic source listener that listens to `Microsoft.ApplicationInsights.Kubernetes.Debugging.Inspect.Observer`. The observer(DiagnosticSource) will emits logs in different levels.

To make diagnostic easier, a default listener that outputs logs into the console is provided and and be enabled like it below before `AddApplicationInsightsKubernetesEnricher` is called:

```csharp
var listener = InspectListener.Instance;
listener.SetMinimumLevel(InspectLevel.Trace);
Inspect.Instance.Observer.SubscribeWithAdapter(listener);
```

When the listener above is enabled, you will see diagnostic logs like this:

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

A similar log shall be output to the terminal.

## Logging Levels

There are 5 levels of the events, each is assigned with a number like:

* Critical - 5
* Error - 4
* Warning - 3
* Information - 2
* Debug - 1
* Trace - 0

When minimum level is set by the listener, issues that has a number greater or equal to it will be print. For example, if the minimum level is set to `Error`:

```csharp
listener.SetMinimumLevel(InspectLevel.Error);
```

Logs with the level of critical or error will be printed to the console.

## About the Diagnostic Source

To find out more details about the [DiagnosticSource], reference [DiagnosticSource Users Guide](https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md#diagnosticsource-users-guide).

If you want to write another diagnostic source listener, here's the basic information:

The DiagnosticSource used in Application Insights is named `ApplicationInsightsKubernetesDiagnosticSource`.

There happen to be 5 predefined events, named after the levels - "Critical", "Error", "Warning", "Information", "Debug" and "Trace".
