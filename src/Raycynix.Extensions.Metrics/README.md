# Raycynix.Extensions.Metrics

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Metrics` contains the core metrics services for Raycynix applications.

## What it contains

- `AddRaycynixMetrics(...)`
- `IMetricsService` registration
- metric wrappers for counters, gauges, and histograms
- optional health check registration

## What it does not contain

- ASP.NET Core middleware
- endpoint mapping
- HTTP request metrics integration

## Usage

```csharp
builder.Services.AddRaycynixMetrics();
```

For ASP.NET Core middleware and endpoint integration, use `Raycynix.Extensions.Metrics.Web`.
