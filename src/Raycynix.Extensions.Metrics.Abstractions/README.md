# Raycynix.Extensions.Metrics.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Metrics.Abstractions` contains the contracts used by the Raycynix metrics packages.

## What it contains

- `IMetricsService`
- `IMetricCounter`
- `IMetricGauge`
- `IMetricHistogram`

## Purpose

This package allows other packages to depend on Raycynix metrics contracts without depending on the metrics implementation package.

## Usage

```csharp
public sealed class CheckoutMetrics(IMetricsService metrics)
{
    private readonly IMetricCounter _orders = metrics.CreateCounter(
        "raycynix_orders_total",
        "Total number of processed orders",
        "status");

    public void RecordSuccess()
    {
        _orders.Increment(labelValues: ["success"]);
    }
}
```
