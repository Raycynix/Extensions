# Raycynix.Extensions.Tracing

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

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

For ASP.NET Core middleware integration, use `Raycynix.Extensions.Tracing.AspNetCore`.
