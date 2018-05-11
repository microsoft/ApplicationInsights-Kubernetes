# Multiple Application Insights Instrumentation Key Example

## Assumption
If you are reading this example, we assume you are familiar with deploy Application Insights Kubernetes with your application in Kubernetes. Refer the basic examples if you do not know how to do that.

## Scenarios
Sometimes, you might have one application sending to different application insight backends. There are several ways to reach the goal. It could be done by using multiple channels ([Reference 1](https://github.com/Microsoft/ApplicationInsights-dotnet/blob/e544ffae4f3188bde01a367364ea3e36f2bf03a9/Test/Microsoft.ApplicationInsights.Test/Shared/Extensibility/TelemetryConfigurationFactoryTest.cs), [Reference 2](https://github.com/Microsoft/ApplicationInsights-dotnet/blob/e544ffae4f3188bde01a367364ea3e36f2bf03a9/Test/Microsoft.ApplicationInsights.Test/Shared/Extensibility/TelemetrySinkTests.cs)) or by building multiple `TelemetryConfiguration` instances to hold multiple iKeys. This example is going to focus on the multiple-iKey scenario, which will be supported from Application Insights Kubernetes 1.0.0-beta8.

Different than single Application Insights Instrumentation Key (iKey), in which case, you are suggested to use the TelemetryClient by ASP.NET Dependency Injection, for multiple-iKey scenario, you will need to build your own Telemetry Clients. The telemetry clients accept a telemetry configuration object, which contains the iKey property. Supporting of calling `EnableKubernetes()` on various `TelemetryConfiguration` is added.

## Key code

Let's talk in code:
```csharp
    // Build a TelemetryClient with iKey1
    TelemetryConfiguration aiConfig = new TelemetryConfiguration("ikey 1", app.ApplicationServices.GetService<ITelemetryChannel>());
    aiConfig.EnableKubernetes();
    TelemetryClient client = new TelemetryClient(aiConfig);
    // Invoking the constructor for the TelemetryInitializer
    client.TrackEvent("Hello");

    // Build a TelemetryClient with iKey1
    TelemetryConfiguration aiConfig2 = new TelemetryConfiguration("iKey 2", app.ApplicationServices.GetService<ITelemetryChannel>());
    aiConfig2.EnableKubernetes();
    TelemetryClient client2 = new TelemetryClient(aiConfig2);
```
Now you can have telemetry clients sending to different application insight backends. Refer [Startup.cs](./Startup.cs) for the full code. 

There are some points worth to mention:
* In this example, we are getting the ITelemetryChannel object from the service provider because
  * It is a `ServerTelemetryChannel` than a, by default, `InMemory` channel.
  * It is reusable for various telemetry configurations.
  * We do not need to worry about the dispose of the channels.

  Alternatively, you could call the constructor on ServerTelemetryChannel to get an instance as well.

* We are still calling `UseApplicationInsights()` in [Program.cs](Program.cs). When an iKey is provided, you will have an additional telemetry client as well in the service provider.

* This is sort of obvious, but since this is the Application Insights Kubernetes example, I have to mention: please do not forget to call `.EnableKubernetes()` on the configuration object to inject Kubernetes information to the telemetry data.

```
Side note: You might notice the very first TrackEvent doesn't come with the Kubernetes info. That is by design because the TelemetryInitializer is non-blocking but it will take an async call to fetch the Kubernetes info.
```
This is how it looks like in two different application insights backend:
![Result Example](./Media/screenshot1.png)

Have fun!
