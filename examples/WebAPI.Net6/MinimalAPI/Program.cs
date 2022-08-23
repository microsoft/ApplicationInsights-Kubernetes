var builder = WebApplication.CreateBuilder(args);

// Enable application insights
builder.Services.AddApplicationInsightsTelemetry();

// Enable application insights for Kubernetes
builder.Services.AddApplicationInsightsKubernetesEnricher(diagnosticLogLevel: LogLevel.Information);

var app = builder.Build();

app.MapGet("/", () => "Hello Minimal API!");

app.Run();
