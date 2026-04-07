# Raycynix.Extensions.Tracing.AspNetCore

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Tracing.AspNetCore` adds ASP.NET Core middleware integration for Raycynix tracing.

## What it contains

- `UseRaycynixTracing(this IApplicationBuilder app)`
- `TracingMiddleware`

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixTracing();

var app = builder.Build();

app.UseRaycynixTracing();

app.Run();
```

This middleware enriches Serilog log context with `TraceId` and `SpanId` resolved from the current `Activity`, and falls back to `HttpContext.TraceIdentifier` when no activity exists.
