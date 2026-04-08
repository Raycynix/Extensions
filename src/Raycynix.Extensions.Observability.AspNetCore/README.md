# Raycynix.Extensions.Observability.AspNetCore

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

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
