# Raycynix.Extensions.Observability.AspNetCore

`Raycynix.Extensions.Observability.AspNetCore` adds ASP.NET Core integration for Raycynix observability and includes the core observability registration.

## What it contains

- `AddRaycynixAspNetCoreObservability(...)`
- `UseRaycynixObservability(this IApplicationBuilder app)`
- `MapRaycynixObservabilityEndpoints(this IEndpointRouteBuilder endpoints)`
- correlation middleware and `HttpClient` correlation propagation

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreObservability();

var app = builder.Build();

app.UseRaycynixObservability();
app.MapRaycynixObservabilityEndpoints();

app.Run();
```

`AddRaycynixAspNetCoreObservability(...)` already calls `AddRaycynixObservability()`, so no extra core registration is required in ASP.NET Core applications.

You can also map custom paths:

```csharp
app.MapRaycynixObservabilityEndpoints("/internal/health", "/internal/metrics");
```

The middleware uses standard `Microsoft.Extensions.Logging` scopes for correlation diagnostics when a logger provider is registered. It does not require `Raycynix.Extensions.Logging`.

By default, the request logging scope includes `CorrelationId`, `TraceId`, `UserId`, `SubjectId`, and `SubjectType` to preserve the previous enrichment behavior. Applications that do not want user or subject identifiers in log scopes can disable that part:

```csharp
builder.Services.AddRaycynixAspNetCoreObservability(options =>
{
    options.IncludeIdentityInLoggingScope = false;
});
```
