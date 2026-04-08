# Raycynix.Extensions.Tracing

`Raycynix.Extensions.Tracing` contains the core tracing services for Raycynix applications.

## What it contains

- `AddRaycynixTracing(...)`
- `ITracer` registration
- `Tracer` implementation built on `ActivitySource`

## What it does not contain

- ASP.NET Core middleware
- `IApplicationBuilder` extensions
- HTTP request pipeline integration

## Usage

```csharp
builder.Services.AddRaycynixTracing();
```

```csharp
public sealed class OrderService(ITracer tracer)
{
    public void Process(string orderId)
    {
        using var activity = tracer.StartTrace("orders.process", new Dictionary<string, string>
        {
            ["order.id"] = orderId
        });

        tracer.SetBaggage("tenant", "alpha");
        tracer.AddTag("operation.type", "command");
    }
}
```

For ASP.NET Core middleware integration, use `Raycynix.Extensions.Tracing.AspNetCore`.
