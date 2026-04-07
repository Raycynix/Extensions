# Raycynix.Extensions.Tracing.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Tracing.Abstractions` contains the contracts used by the Raycynix tracing packages.

## What it contains

- `ITracer`

## Purpose

This package allows other packages to depend on Raycynix tracing contracts without depending on the tracing implementation package.

## Usage

```csharp
public sealed class CheckoutWorkflow(ITracer tracer)
{
    public void Run()
    {
        using var activity = tracer.StartTrace("checkout.run");
        tracer.AddTag("workflow", "checkout");
    }
}
```
