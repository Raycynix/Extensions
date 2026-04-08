# Raycynix.Extensions.Metrics.AspNetCore

`Raycynix.Extensions.Metrics.AspNetCore` adds ASP.NET Core middleware and endpoint integration for Raycynix metrics.

## What it contains

- `UseRaycynixMetrics(this IApplicationBuilder app)`
- `MapRaycynixMetrics(this IEndpointRouteBuilder endpoints)`

## appsettings.json

```json
{
  "MetricsConfiguration": {
    "UsePrometheus": true,
    "MetricsEndpoint": "/metrics"
  }
}
```

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixMetrics(builder.Configuration);

var app = builder.Build();

app.UseRaycynixMetrics();
app.MapRaycynixMetrics();

app.Run();
```
