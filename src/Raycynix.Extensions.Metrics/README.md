# Raycynix.Extensions.Metrics

`Raycynix.Extensions.Metrics` contains the core metrics services for Raycynix applications.

## What it contains

- `AddRaycynixMetrics(...)`
- `AddRaycynixMetrics(IConfiguration, ...)`
- `IMetricsService` registration
- metric wrappers for counters, gauges, and histograms
- optional health check registration

## What it does not contain

- ASP.NET Core middleware
- endpoint mapping
- HTTP request metrics integration

## appsettings.json

```json
{
  "MetricsConfiguration": {
    "UsePrometheus": true,
    "MetricsEndpoint": "/metrics",
    "UseHealthChecks": true
  }
}
```

## Usage

```csharp
builder.Services.AddRaycynixMetrics(builder.Configuration);
```

```csharp
builder.Services.AddRaycynixMetrics(builder.Configuration, options =>
{
    options.MetricsEndpoint = "/internal/metrics";
});
```

For ASP.NET Core middleware and endpoint integration, use `Raycynix.Extensions.Metrics.AspNetCore`.
