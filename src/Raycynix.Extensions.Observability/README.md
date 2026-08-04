# Raycynix.Extensions.Observability

`Raycynix.Extensions.Observability` contains the core observability composition for Raycynix applications.

## What it contains

- `AddRaycynixObservability(...)`
- registration of tracing and metrics services
- `IOperationContext` registration

## What it does not contain

- ASP.NET Core middleware
- endpoint mapping
- `HttpClient` correlation propagation

## Usage

```csharp
builder.Services.AddRaycynixObservability();
```

```csharp
public sealed class CheckoutHandler(
    IOperationContext operationContext,
    Microsoft.Extensions.Logging.ILogger<CheckoutHandler> logger,
    Raycynix.Extensions.Tracing.Abstractions.Interfaces.ITracer tracer)
{
    public void Handle()
    {
        using var activity = tracer.StartTrace("checkout.handle");

        logger.LogInformation(
            "Handling checkout. CorrelationId:{CorrelationId} TraceId:{TraceId}",
            operationContext.CorrelationId,
            operationContext.TraceId);
    }
}
```

`AddRaycynixObservability()` does not register `Raycynix.Extensions.Serilog`. Applications can use any provider that works with `Microsoft.Extensions.Logging`.

For ASP.NET Core integration, use `Raycynix.Extensions.Observability.AspNetCore`.
`AddRaycynixAspNetCoreObservability(...)` already calls `AddRaycynixObservability()` for you.
