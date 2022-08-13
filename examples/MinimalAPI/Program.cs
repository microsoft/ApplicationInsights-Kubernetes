var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json, including application insights connection string.
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

// Enable application insights
builder.Services.AddApplicationInsightsTelemetry();

// Enable application insights for Kubernetes
builder.Services.AddApplicationInsightsKubernetesEnricher(LogLevel.Trace);

var app = builder.Build();

app.MapGet("/", () => "Hello Minimal API!");

app.Run();
