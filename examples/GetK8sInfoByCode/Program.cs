using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Adding application insights SDK and the telemetry enricher.
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddApplicationInsightsKubernetesEnricher(diagnosticLogLevel: LogLevel.Information);

var app = builder.Build();

// Inject IK8sInfoService to use k8s info.
app.MapGet("/", ([FromServices] IK8sInfoService k8SInfoService) =>
{
    if (k8SInfoService.TryGetK8sInfo(out IK8sEnvironment? k8sInfo))
    {
        // When getting the info success, k8sInfo object is ready for consumption.
        Console.WriteLine(k8sInfo);
    }
    else
    {
        // The k8s info is not there.
        Console.WriteLine("No k8s info. Am I running inside a k8s cluster?");
    }
    return new OkObjectResult(k8sInfo);
});

app.Run();
